import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
    useQuizHub,
    QuestionPayload,
    AnswerRevealedPayload,
} from '../hooks/useQuizHub';
import './ParticipantScreen.css';

type Phase = 'joining' | 'waiting' | 'answering' | 'submitted' | 'revealed' | 'ended';

const ParticipantScreen: React.FC = () => {
    const { connected, invoke, on, error: hubError } = useQuizHub();

    // Join
    const [roomCode, setRoomCode] = useState('');
    const [displayName, setDisplayName] = useState('');
    const [joined, setJoined] = useState(false);
    const [hostName, setHostName] = useState('');
    const [joinError, setJoinError] = useState<string | null>(null);

    // Question
    const [currentQuestion, setCurrentQuestion] = useState<QuestionPayload | null>(null);
    const [selectedAnswer, setSelectedAnswer] = useState<number | null>(null);
    const [phase, setPhase] = useState<Phase>('joining');

    // Reveal
    const [revealData, setRevealData] = useState<AnswerRevealedPayload | null>(null);
    const [myScore, setMyScore] = useState(0);

    // Media
    const mediaRef = useRef<HTMLVideoElement | HTMLAudioElement | null>(null);

    // ── Hub events ──────────────────────────────────────────────────
    useEffect(() => {
        const unsubs = [
            on('JoinedRoom', (data: { roomCode: string; hostName: string }) => {
                setJoined(true);
                setHostName(data.hostName);
                setPhase('waiting');
            }),
            on('Error', (msg: string) => {
                setJoinError(msg);
            }),
            on('NewQuestion', (question: QuestionPayload) => {
                if (mediaRef.current) mediaRef.current.pause();
                setCurrentQuestion(question);
                setSelectedAnswer(null);
                setRevealData(null);
                setPhase('answering');
            }),
            on('AnswerReceived', () => {
                setPhase('submitted');
            }),
            on('AnswerRevealed', (data: AnswerRevealedPayload) => {
                setRevealData(data);
                setPhase('revealed');
                const me = data.scoreboard.find((s) => s.displayName === displayName);
                if (me) setMyScore(me.score);
            }),
            on('QuizEnded', () => {
                setPhase('ended');
            }),
        ];
        return () => unsubs.forEach((u) => u?.());
    }, [on, displayName]);

    // ── Auto-play media ─────────────────────────────────────────────
    useEffect(() => {
        if (phase === 'answering' && mediaRef.current) {
            mediaRef.current.currentTime = 0;
            mediaRef.current.play().catch(() => {});
        }
    }, [phase, currentQuestion]);

    // ── Actions ─────────────────────────────────────────────────────
    const joinRoom = useCallback(async () => {
        if (!roomCode.trim() || !displayName.trim()) return;
        setJoinError(null);
        try {
            await invoke('JoinRoom', roomCode.toUpperCase().trim(), displayName.trim());
        } catch (err: any) {
            setJoinError(err.message);
        }
    }, [roomCode, displayName, invoke]);

    const submitAnswer = useCallback(
        async (answerId: number) => {
            setSelectedAnswer(answerId);
            try {
                await invoke('SubmitAnswer', roomCode.toUpperCase().trim(), answerId);
            } catch (err: any) {
                console.error('Submit error:', err);
            }
        },
        [roomCode, invoke]
    );

    // ── Media renderer ──────────────────────────────────────────────
    const renderMedia = (q: QuestionPayload) => {
        if (!q.mediaUrl) return null;
        const url = q.mediaUrl.startsWith('http') ? q.mediaUrl : `${process.env.REACT_APP_API_URL}${q.mediaUrl}`;

        switch (q.mediaType) {
            case 'image':
                return <img src={url} alt="" className="p-media" />;
            case 'video':
                return (
                    <video ref={(el) => { mediaRef.current = el; }} src={url} className="p-media" autoPlay playsInline />
                );
            case 'audio':
                return (
                    <audio ref={(el) => { mediaRef.current = el; }} src={url} className="p-media p-media--audio" autoPlay />
                );
            default:
                return null;
        }
    };

    // ════════════════════════════════════════════════════════════════
    // RENDER
    // ════════════════════════════════════════════════════════════════

    // ── JOIN ─────────────────────────────────────────────────────────
    if (phase === 'joining') {
        return (
            <div className="p-screen p-screen--join">
                <div className="join-card">
                    <h1>🎯 Join Quiz</h1>
                    <p className="join-sub">Enter the room code shown on the main screen</p>

                    <div className="join-field">
                        <label>Room Code</label>
                        <input
                            type="text"
                            maxLength={6}
                            placeholder="ABC123"
                            value={roomCode}
                            onChange={(e) => setRoomCode(e.target.value.toUpperCase())}
                            className="join-input join-input--code"
                        />
                    </div>

                    <div className="join-field">
                        <label>Your Name</label>
                        <input
                            type="text"
                            maxLength={20}
                            placeholder="Enter your name"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            className="join-input"
                        />
                    </div>

                    {joinError && <p className="join-err">{joinError}</p>}
                    {hubError && <p className="join-err">Connection error: {hubError}</p>}

                    <button
                        className="btn btn--primary btn--full"
                        disabled={!connected || !roomCode.trim() || !displayName.trim()}
                        onClick={joinRoom}
                    >
                        {connected ? 'Join' : 'Connecting...'}
                    </button>
                </div>
            </div>
        );
    }

    // ── WAITING ─────────────────────────────────────────────────────
    if (phase === 'waiting') {
        return (
            <div className="p-screen p-screen--waiting">
                <div className="waiting-box">
                    <div className="pulse" />
                    <h2>You're in!</h2>
                    <p>Waiting for {hostName || 'the host'} to start...</p>
                    <p className="waiting-name">{displayName}</p>
                </div>
            </div>
        );
    }

    // ── ANSWERING ───────────────────────────────────────────────────
    if (phase === 'answering' && currentQuestion) {
        const colors = ['ans--a', 'ans--b', 'ans--c', 'ans--d'];
        return (
            <div className="p-screen p-screen--answering">
                <div className="p-question">{currentQuestion.questionText}</div>
                {renderMedia(currentQuestion)}
                <div className="answer-grid">
                    {currentQuestion.answers.map((opt, i) => (
                        <button
                            key={opt.id}
                            className={`answer-btn ${colors[i]}`}
                            onClick={() => submitAnswer(opt.id)}
                        >
                            <span className="answer-btn__letter">{String.fromCharCode(65 + i)}</span>
                            <span className="answer-btn__text">{opt.answerText}</span>
                        </button>
                    ))}
                </div>
            </div>
        );
    }

    // ── SUBMITTED ───────────────────────────────────────────────────
    if (phase === 'submitted') {
        return (
            <div className="p-screen p-screen--submitted">
                <div className="submitted-box">
                    <div className="check-circle">✓</div>
                    <h2>Answer Submitted!</h2>
                    <p>Waiting for the host to reveal...</p>
                </div>
            </div>
        );
    }

    // ── REVEALED ────────────────────────────────────────────────────
    if (phase === 'revealed' && revealData) {
        const myResult = revealData.results.find((r) => r.participantName === displayName);
        const isCorrect = myResult?.isCorrect ?? false;
        const correctOpt = currentQuestion?.answers.find((a) => a.id === revealData.correctAnswerId);

        return (
            <div className={`p-screen p-screen--revealed ${isCorrect ? 'p-screen--correct' : 'p-screen--wrong'}`}>
                <div className="reveal-box">
                    <div className="reveal-icon">{isCorrect ? '🎉' : '😔'}</div>
                    <h2>{isCorrect ? 'Correct!' : 'Not quite!'}</h2>
                    <p className="reveal-answer">Answer: {correctOpt?.answerText}</p>
                    <div className="reveal-score">
                        <span className="reveal-score__val">{myScore}</span>
                        <span className="reveal-score__label">Your Score</span>
                    </div>
                </div>
            </div>
        );
    }

    // ── ENDED ───────────────────────────────────────────────────────
    if (phase === 'ended') {
        return (
            <div className="p-screen p-screen--ended">
                <h1>🏆 Quiz Over!</h1>
                <div className="final-score">
                    <span className="final-score__val">{myScore}</span>
                    <span className="final-score__label">Final Score</span>
                </div>
                <p>Thanks for playing, {displayName}!</p>
            </div>
        );
    }

    return <div className="p-screen">Loading...</div>;
};

export default ParticipantScreen;
