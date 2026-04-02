import React, { useState, useEffect, useCallback, useRef } from 'react';
import axios from 'axios';
import {
    useQuizHub,
    QuestionPayload,
    AnswerRevealedPayload,
} from '../hooks/useQuizHub';
import './HostScreen.css';

const API = 'http://localhost:5291/api';

interface QuizSummary {
    id: number;
    categoryName: string;
    title: string;
    questionCount: number;
}

interface FullQuiz {
    id: number;
    categoryName: string;
    title: string;
    questions: QuestionPayload[];
}

type Phase = 'lobby' | 'question' | 'revealed' | 'ended';

const HostScreen: React.FC = () => {
    const { connected, invoke, on } = useQuizHub();

    // Room
    const [roomCode, setRoomCode] = useState<string | null>(null);
    const [participants, setParticipants] = useState<string[]>([]);
    const [participantCount, setParticipantCount] = useState(0);

    // Quiz
    const [quizList, setQuizList] = useState<QuizSummary[]>([]);
    const [selectedQuiz, setSelectedQuiz] = useState<FullQuiz | null>(null);
    const [questionIndex, setQuestionIndex] = useState(-1);
    const [currentQuestion, setCurrentQuestion] = useState<QuestionPayload | null>(null);

    // Answers
    const [answerCount, setAnswerCount] = useState(0);
    const [revealedResult, setRevealedResult] = useState<AnswerRevealedPayload | null>(null);
    const [phase, setPhase] = useState<Phase>('lobby');

    // Media auto-play
    const mediaRef = useRef<HTMLVideoElement | HTMLAudioElement | null>(null);

    // ── Fetch quiz list from your existing API ──────────────────────
    useEffect(() => {
        const token = localStorage.getItem('token');
        axios
            .get(`${API}/quiz/categories`, {
                headers: { Authorization: `Bearer ${token}` },
            })
            .then((res) => setQuizList(res.data))
            .catch(console.error);
    }, []);

    // ── Hub events ──────────────────────────────────────────────────
    useEffect(() => {
        const unsubs = [
            on('ParticipantJoined', (data: { displayName: string; totalParticipants: number }) => {
                setParticipants((prev) => [...prev, data.displayName]);
                setParticipantCount(data.totalParticipants);
            }),
            on('ParticipantLeft', (data: { displayName: string; totalParticipants: number }) => {
                setParticipants((prev) => prev.filter((p) => p !== data.displayName));
                setParticipantCount(data.totalParticipants);
            }),
            on('AnswerSubmitted', (data: { answerCount: number }) => {
                setAnswerCount(data.answerCount);
            }),
            on('AnswerRevealed', (data: AnswerRevealedPayload) => {
                setRevealedResult(data);
                setPhase('revealed');
            }),
        ];
        return () => unsubs.forEach((u) => u?.());
    }, [on]);

    // ── Auto-play / stop media ──────────────────────────────────────
    useEffect(() => {
        if (mediaRef.current) {
            mediaRef.current.currentTime = 0;
            mediaRef.current.play().catch(() => {});
        }
        return () => {
            if (mediaRef.current) {
                mediaRef.current.pause();
            }
        };
    }, [currentQuestion]);

    // ── Actions ─────────────────────────────────────────────────────
    const selectAndCreateRoom = useCallback(
        async (quizSummary: QuizSummary) => {
            // Fetch full quiz with questions + answers
            const token = localStorage.getItem('token');
            const res = await axios.get(`${API}/quiz/${quizSummary.id}`, {
                headers: { Authorization: `Bearer ${token}` },
            });
            setSelectedQuiz(res.data);
            setQuestionIndex(-1);

            // Create SignalR room
            const code = await invoke<string>('CreateRoom', localStorage.getItem('email') || 'Host');
            setRoomCode(code);
        },
        [invoke]
    );

    const nextQuestion = useCallback(async () => {
        if (!selectedQuiz || !roomCode) return;
        const nextIdx = questionIndex + 1;

        if (nextIdx >= selectedQuiz.questions.length) {
            await invoke('EndQuiz', roomCode);
            setPhase('ended');
            return;
        }

        const q = selectedQuiz.questions[nextIdx];
        setQuestionIndex(nextIdx);
        setCurrentQuestion(q);
        setAnswerCount(0);
        setRevealedResult(null);
        setPhase('question');

        await invoke('SendQuestion', roomCode, q);
    }, [selectedQuiz, roomCode, questionIndex, invoke]);

    const revealAnswer = useCallback(async () => {
        if (!roomCode || !currentQuestion) return;
        const correct = currentQuestion.answers.find((a) => a.isCorrect);
        if (!correct) return;
        await invoke('RevealAnswer', roomCode, correct.id);
    }, [roomCode, currentQuestion, invoke]);

    // ── Media renderer ──────────────────────────────────────────────
    const renderMedia = (q: QuestionPayload) => {
        if (!q.mediaUrl) return null;
        const url = q.mediaUrl.startsWith('http') ? q.mediaUrl : `http://localhost:5291${q.mediaUrl}`;

        switch (q.mediaType) {
            case 'image':
                return <img src={url} alt="" className="host-media" />;
            case 'video':
                return (
                    <video
                        ref={(el) => { mediaRef.current = el; }}
                        src={url}
                        className="host-media"
                        controls
                        autoPlay
                    />
                );
            case 'audio':
                return (
                    <audio
                        ref={(el) => { mediaRef.current = el; }}
                        src={url}
                        className="host-media host-media--audio"
                        controls
                        autoPlay
                    />
                );
            default:
                return null;
        }
    };

    // ════════════════════════════════════════════════════════════════
    // RENDER
    // ════════════════════════════════════════════════════════════════

    // ── LOBBY ───────────────────────────────────────────────────────
    if (phase === 'lobby') {
        return (
            <div className="host-screen">
                <div className="host-header">
                    <h1>🎯 Moderated Quiz</h1>
                    {connected ? (
                        <span className="status-badge status-badge--ok">Connected</span>
                    ) : (
                        <span className="status-badge status-badge--err">Disconnected</span>
                    )}
                </div>

                {!roomCode ? (
                    <div className="host-lobby">
                        <h2>Select a Quiz to Host</h2>
                        <div className="quiz-grid">
                            {quizList.map((q) => (
                                <button key={q.id} className="quiz-card" onClick={() => selectAndCreateRoom(q)}>
                                    <span className="quiz-card__cat">{q.categoryName}</span>
                                    <span className="quiz-card__title">{q.title}</span>
                                    <span className="quiz-card__count">{q.questionCount} questions</span>
                                </button>
                            ))}
                        </div>
                    </div>
                ) : (
                    <div className="host-waiting">
                        <div className="room-code-box">
                            <span className="room-code-label">Room Code</span>
                            <span className="room-code-value">{roomCode}</span>
                            <span className="room-code-hint">
                                Enter this code on your tablet to join
                            </span>
                        </div>

                        <div className="participant-list">
                            <h3>Participants ({participantCount})</h3>
                            <div className="participant-chips">
                                {participants.map((name, i) => (
                                    <span key={i} className="chip">{name}</span>
                                ))}
                                {participantCount === 0 && (
                                    <span className="chip chip--empty">Waiting for players...</span>
                                )}
                            </div>
                        </div>

                        {participantCount > 0 && (
                            <button className="btn btn--primary btn--lg" onClick={nextQuestion}>
                                Start First Question →
                            </button>
                        )}
                    </div>
                )}
            </div>
        );
    }

    // ── ACTIVE QUESTION ─────────────────────────────────────────────
    if (phase === 'question' && currentQuestion) {
        const optionColors = ['opt--a', 'opt--b', 'opt--c', 'opt--d'];
        return (
            <div className="host-screen">
                <div className="host-topbar">
                    <span>{selectedQuiz?.title}</span>
                    <span>
                        Question {questionIndex + 1} / {selectedQuiz?.questions.length}
                    </span>
                    <span>Room: {roomCode}</span>
                </div>

                <div className="host-question-area">
                    {renderMedia(currentQuestion)}
                    <h2 className="host-q-text">{currentQuestion.questionText}</h2>

                    <div className="host-options">
                        {currentQuestion.answers.map((opt, i) => (
                            <div key={opt.id} className={`host-opt ${optionColors[i]}`}>
                                <span className="host-opt__letter">{String.fromCharCode(65 + i)}</span>
                                <span>{opt.answerText}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="host-bottom">
                    <div className="answer-counter">
                        <span className="answer-counter__num">{answerCount}</span>
                        <span>/ {participantCount} answered</span>
                    </div>
                    <button className="btn btn--accent" onClick={revealAnswer}>
                        Reveal Answer
                    </button>
                </div>
            </div>
        );
    }

    // ── REVEALED ────────────────────────────────────────────────────
    if (phase === 'revealed' && revealedResult && currentQuestion) {
        const optionColors = ['opt--a', 'opt--b', 'opt--c', 'opt--d'];
        return (
            <div className="host-screen">
                <div className="host-topbar">
                    <span>{selectedQuiz?.title}</span>
                    <span>
                        Question {questionIndex + 1} / {selectedQuiz?.questions.length}
                    </span>
                </div>

                <div className="host-question-area">
                    <h2 className="host-q-text">{currentQuestion.questionText}</h2>

                    <div className="host-options">
                        {currentQuestion.answers.map((opt, i) => (
                            <div
                                key={opt.id}
                                className={`host-opt ${optionColors[i]} ${
                                    opt.id === revealedResult.correctAnswerId
                                        ? 'host-opt--correct'
                                        : 'host-opt--dim'
                                }`}
                            >
                                <span className="host-opt__letter">{String.fromCharCode(65 + i)}</span>
                                <span>{opt.answerText}</span>
                                {opt.id === revealedResult.correctAnswerId && (
                                    <span className="host-opt__check">✓</span>
                                )}
                            </div>
                        ))}
                    </div>

                    <div className="host-stats">
                        <div className="stat">
                            <span className="stat__val">{revealedResult.correctCount}</span>
                            <span className="stat__label">Correct</span>
                        </div>
                        <div className="stat">
                            <span className="stat__val">
                                {revealedResult.totalAnswers - revealedResult.correctCount}
                            </span>
                            <span className="stat__label">Incorrect</span>
                        </div>
                    </div>

                    <div className="scoreboard">
                        <h3>Scoreboard</h3>
                        {revealedResult.scoreboard.map((entry, i) => (
                            <div key={i} className={`sb-row ${i === 0 ? 'sb-row--first' : ''}`}>
                                <span className="sb-row__rank">#{i + 1}</span>
                                <span className="sb-row__name">{entry.displayName}</span>
                                <span className="sb-row__score">{entry.score}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="host-bottom">
                    <button className="btn btn--primary btn--lg" onClick={nextQuestion}>
                        {questionIndex + 1 < (selectedQuiz?.questions.length ?? 0)
                            ? 'Next Question →'
                            : 'Finish Quiz'}
                    </button>
                </div>
            </div>
        );
    }

    // ── ENDED ───────────────────────────────────────────────────────
    if (phase === 'ended') {
        return (
            <div className="host-screen host-screen--ended">
                <h1>🏆 Quiz Complete!</h1>
                {revealedResult && (
                    <div className="scoreboard scoreboard--final">
                        <h2>Final Scores</h2>
                        {revealedResult.scoreboard.map((entry, i) => (
                            <div key={i} className={`sb-row ${i === 0 ? 'sb-row--first' : ''}`}>
                                <span className="sb-row__rank">#{i + 1}</span>
                                <span className="sb-row__name">{entry.displayName}</span>
                                <span className="sb-row__score">{entry.score}</span>
                            </div>
                        ))}
                    </div>
                )}
                <button className="btn btn--primary" onClick={() => window.location.reload()}>
                    Host Another Quiz
                </button>
            </div>
        );
    }

    return <div className="host-screen">Loading...</div>;
};

export default HostScreen;
