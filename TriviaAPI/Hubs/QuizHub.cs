using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TriviaAPI.Models;

namespace TriviaAPI.Hubs;

// ── Room & session models (in-memory) ───────────────────────────────

public class QuizRoom
{
    public string RoomCode { get; set; } = string.Empty;
    public string HostConnectionId { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int? CurrentQuizId { get; set; }
    public int CurrentQuestionIndex { get; set; } = -1;
    public QuestionDto? CurrentQuestion { get; set; }
    public ConcurrentDictionary<string, ParticipantInfo> Participants { get; } = new();
    public ConcurrentDictionary<string, AnswerSubmission> Answers { get; } = new();
    public bool IsActive { get; set; } = true;
}

public class ParticipantInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class AnswerSubmission
{
    public string ParticipantName { get; set; } = string.Empty;
    public int SelectedAnswerId { get; set; }
    public DateTime SubmittedAt { get; set; }
}

// ── SignalR Hub ─────────────────────────────────────────────────────

[Authorize]
public class QuizHub : Hub
{
    private static readonly ConcurrentDictionary<string, QuizRoom> Rooms = new();
    private static readonly ConcurrentDictionary<string, string> ConnectionToRoom = new();

    // ─── HOST: Create a room ────────────────────────────────────────

    public async Task<string> CreateRoom(string hostName)
    {
        var roomCode = GenerateRoomCode();
        var room = new QuizRoom
        {
            RoomCode = roomCode,
            HostConnectionId = Context.ConnectionId,
            HostName = hostName
        };

        Rooms[roomCode] = room;
        ConnectionToRoom[Context.ConnectionId] = roomCode;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.Caller.SendAsync("RoomCreated", roomCode);

        return roomCode;
    }

    // ─── HOST: Push a question to all participants ──────────────────
    // Receives the full QuestionDto (with IsCorrect) from the host.
    // Strips IsCorrect before sending to participants.

    public async Task SendQuestion(string roomCode, QuestionDto question)
    {
        if (!Rooms.TryGetValue(roomCode, out var room)) return;
        if (room.HostConnectionId != Context.ConnectionId) return;

        room.Answers.Clear();
        room.CurrentQuestion = question;
        room.CurrentQuestionIndex++;

        // Full question to host monitor
        await Clients.Caller.SendAsync("QuestionDisplayed", question);

        // Stripped version to participant tablets (no IsCorrect!)
        var participantPayload = new
        {
            question.Id,
            question.QuestionText,
            question.MediaUrl,
            question.MediaType,
            Answers = question.Answers.Select(a => new
            {
                a.Id,
                a.AnswerText
            }).ToList()
        };

        await Clients.GroupExcept(roomCode, new[] { Context.ConnectionId })
                     .SendAsync("NewQuestion", participantPayload);
    }

    // ─── HOST: Reveal the correct answer ────────────────────────────

    public async Task RevealAnswer(string roomCode, int correctAnswerId)
    {
        if (!Rooms.TryGetValue(roomCode, out var room)) return;
        if (room.HostConnectionId != Context.ConnectionId) return;

        var results = room.Answers.Values.Select(a => new
        {
            a.ParticipantName,
            a.SelectedAnswerId,
            IsCorrect = a.SelectedAnswerId == correctAnswerId,
            a.SubmittedAt
        }).ToList();

        int totalAnswers = results.Count;
        int correctCount = results.Count(r => r.IsCorrect);

        // Update participant scores
        foreach (var correct in results.Where(r => r.IsCorrect))
        {
            var p = room.Participants.Values
                .FirstOrDefault(x => x.DisplayName == correct.ParticipantName);
            if (p != null) p.Score++;
        }

        var scoreboard = room.Participants.Values
            .OrderByDescending(p => p.Score)
            .Select(p => new { p.DisplayName, p.Score })
            .ToList();

        await Clients.Group(roomCode).SendAsync("AnswerRevealed", new
        {
            CorrectAnswerId = correctAnswerId,
            TotalAnswers = totalAnswers,
            CorrectCount = correctCount,
            Results = results,
            Scoreboard = scoreboard
        });
    }

    // ─── HOST: End the quiz session ─────────────────────────────────

    public async Task EndQuiz(string roomCode)
    {
        if (!Rooms.TryGetValue(roomCode, out var room)) return;
        if (room.HostConnectionId != Context.ConnectionId) return;

        var finalScoreboard = room.Participants.Values
            .OrderByDescending(p => p.Score)
            .Select(p => new { p.DisplayName, p.Score })
            .ToList();

        await Clients.Group(roomCode).SendAsync("QuizEnded", finalScoreboard);
        room.IsActive = false;
        Rooms.TryRemove(roomCode, out _);
    }

    // ─── PARTICIPANT: Join a room ───────────────────────────────────

    public async Task JoinRoom(string roomCode, string displayName)
    {
        if (!Rooms.TryGetValue(roomCode, out var room))
        {
            await Clients.Caller.SendAsync("Error", "Room not found. Check the code and try again.");
            return;
        }

        if (!room.IsActive)
        {
            await Clients.Caller.SendAsync("Error", "This quiz session has ended.");
            return;
        }

        var participant = new ParticipantInfo
        {
            ConnectionId = Context.ConnectionId,
            DisplayName = displayName,
            Score = 0
        };

        room.Participants[Context.ConnectionId] = participant;
        ConnectionToRoom[Context.ConnectionId] = roomCode;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Notify host
        await Clients.Client(room.HostConnectionId).SendAsync("ParticipantJoined", new
        {
            participant.DisplayName,
            TotalParticipants = room.Participants.Count
        });

        // Confirm to participant
        await Clients.Caller.SendAsync("JoinedRoom", new
        {
            room.RoomCode,
            room.HostName
        });

        // If a question is already active, send it to the late joiner
        if (room.CurrentQuestion != null)
        {
            var payload = new
            {
                room.CurrentQuestion.Id,
                room.CurrentQuestion.QuestionText,
                room.CurrentQuestion.MediaUrl,
                room.CurrentQuestion.MediaType,
                Answers = room.CurrentQuestion.Answers
                    .Select(a => new { a.Id, a.AnswerText }).ToList()
            };
            await Clients.Caller.SendAsync("NewQuestion", payload);
        }
    }

    // ─── PARTICIPANT: Submit an answer ──────────────────────────────

    public async Task SubmitAnswer(string roomCode, int answerId)
    {
        if (!Rooms.TryGetValue(roomCode, out var room)) return;
        if (!room.Participants.TryGetValue(Context.ConnectionId, out var participant)) return;

        // Prevent double-submit
        if (room.Answers.ContainsKey(Context.ConnectionId)) return;

        var submission = new AnswerSubmission
        {
            ParticipantName = participant.DisplayName,
            SelectedAnswerId = answerId,
            SubmittedAt = DateTime.UtcNow
        };

        room.Answers[Context.ConnectionId] = submission;

        await Clients.Caller.SendAsync("AnswerReceived", answerId);

        await Clients.Client(room.HostConnectionId).SendAsync("AnswerSubmitted", new
        {
            participant.DisplayName,
            AnswerCount = room.Answers.Count,
            TotalParticipants = room.Participants.Count
        });
    }

    // ─── Lifecycle ──────────────────────────────────────────────────

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionToRoom.TryRemove(Context.ConnectionId, out var roomCode))
        {
            if (Rooms.TryGetValue(roomCode, out var room))
            {
                if (room.HostConnectionId == Context.ConnectionId)
                {
                    await Clients.Group(roomCode).SendAsync("QuizEnded",
                        room.Participants.Values
                            .OrderByDescending(p => p.Score)
                            .Select(p => new { p.DisplayName, p.Score })
                            .ToList());
                    Rooms.TryRemove(roomCode, out _);
                }
                else
                {
                    room.Participants.TryRemove(Context.ConnectionId, out var left);
                    await Clients.Client(room.HostConnectionId).SendAsync("ParticipantLeft", new
                    {
                        DisplayName = left?.DisplayName,
                        TotalParticipants = room.Participants.Count
                    });
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }
}