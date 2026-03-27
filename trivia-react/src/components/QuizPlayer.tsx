import React, {useState, useEffect, useRef} from 'react';
import {useParams, useNavigate} from 'react-router-dom';
import {quizAPI} from '../services/api';
import {QuizDto, Question} from '../types';
import './QuizPlayer.css';

const QuizPlayer: React.FC = () => {
    const {id} = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [quiz, setQuiz] = useState<QuizDto | null>(null);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const [isAutoPlay, setIsAutoPlay] = useState(false);
    const [showResult, setShowResult] = useState(false);
    const [selectedAnswer, setSelectedAnswer] = useState<number | null>(null);

    const mediaRef = useRef<HTMLVideoElement | HTMLAudioElement>(null);
    const autoPlayTimerRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
        loadQuiz();
    }, [id]);

    useEffect(() => {
        // Auto-play media when question changes
        if (mediaRef.current) {
            mediaRef.current.play().catch(() => {
                // Auto-play might be blocked by browser
                console.log('Auto-play blocked');
            });
        }

        return () => {
            // Stop media when moving to next question
            if (mediaRef.current) {
                mediaRef.current.pause();
                mediaRef.current.currentTime = 0;
            }
        };
    }, [currentQuestionIndex]);

    useEffect(() => {
        // Auto-play mode timer
        if (isAutoPlay && quiz && currentQuestionIndex < quiz.questions.length) {
            const currentQuestion = quiz.questions[currentQuestionIndex];
            const correctAnswer = currentQuestion.answers.find(a => a.isCorrect);

            autoPlayTimerRef.current = setTimeout(() => {
                if (correctAnswer) {
                    handleAnswerClick(correctAnswer.id, true);
                }
            }, 8000); // Auto-advance after 5 seconds
        }

        return () => {
            if (autoPlayTimerRef.current) {
                clearTimeout(autoPlayTimerRef.current);
            }
        };
    }, [isAutoPlay, currentQuestionIndex, quiz]);

    const loadQuiz = async () => {
        try {
            const data = await quizAPI.getQuiz(Number(id));
            setQuiz(data);
        } catch (err) {
            alert('Failed to load quiz');
            navigate('/categories');
        } finally {
            setIsLoading(false);
        }
    };

    const handleAnswerClick = (answerId: number, isCorrect: boolean) => {
        if (selectedAnswer !== null) return; // Already answered

        setSelectedAnswer(answerId);

        if (isCorrect) {
            setScore(score + 1);
        }

        // Move to next question after short delay
        setTimeout(() => {
            if (quiz && currentQuestionIndex < quiz.questions.length - 1) {
                setCurrentQuestionIndex(currentQuestionIndex + 1);
                setSelectedAnswer(null);
            } else {
                setShowResult(true);
            }
        }, 3000);
    };

    const toggleAutoPlay = () => {
        setIsAutoPlay(!isAutoPlay);
    };

    const restartQuiz = () => {
        setCurrentQuestionIndex(0);
        setScore(0);
        setShowResult(false);
        setSelectedAnswer(null);
        setIsAutoPlay(false);
    };

    if (isLoading) {
        return <div className="loading">Loading quiz...</div>;
    }

    if (!quiz) {
        return <div className="error">Quiz not found</div>;
    }

    if (showResult) {
        const percentage = Math.round((score / quiz.questions.length) * 100);
        return (
            <div className="result-container">
                <div className="result-card">
                    <h1>Quiz Complete! 🎉</h1>
                    <div className="score-display">
                        <div className="score-big">{score}</div>
                        <div className="score-text">out of {quiz.questions.length}</div>
                    </div>
                    <div className="percentage">{percentage}%</div>
                    <div className="result-message">
                        {percentage >= 80 && "Excellent! You're a trivia master! 🏆"}
                        {percentage >= 60 && percentage < 80 && "Great job! Keep it up! 👏"}
                        {percentage >= 40 && percentage < 60 && "Not bad! Practice makes perfect! 💪"}
                        {percentage < 40 && "Keep trying! You'll get better! 📚"}
                    </div>
                    <div className="result-buttons">
                        <button onClick={restartQuiz} className="restart-btn">
                            Play Again
                        </button>
                        <button onClick={() => navigate('/categories')} className="home-btn">
                            Back to Categories
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    const currentQuestion = quiz.questions[currentQuestionIndex];
    const API_BASE_URL = 'http://localhost:5291';

    return (
        <div className="quiz-player-container">
            <div className="quiz-header">
                <h1>{quiz.categoryName}</h1>
                <div className="quiz-progress">
                    Question {currentQuestionIndex + 1} of {quiz.questions.length}
                </div>
                <div className="quiz-score">Score: {score}</div>
            </div>

            <div className="auto-play-toggle">
                <label>
                    <input
                        type="checkbox"
                        checked={isAutoPlay}
                        onChange={toggleAutoPlay}
                    />
                    Auto-Play Mode
                </label>
            </div>

            <div className="question-card">
                {/* Media Display */}
                {currentQuestion.mediaUrl && (
                    <div className="media-container">
                        {currentQuestion.mediaType === 'image' && (
                            <img
                                src={`${API_BASE_URL}${currentQuestion.mediaUrl}`}
                                alt="Question media"
                                className="question-media"
                            />
                        )}
                        {currentQuestion.mediaType === 'audio' && (
                            <audio
                                ref={mediaRef as React.RefObject<HTMLAudioElement>}
                                controls
                                className="question-media"
                            >
                                <source src={`${API_BASE_URL}${currentQuestion.mediaUrl}`}/>
                            </audio>
                        )}
                        {currentQuestion.mediaType === 'video' && (
                            <video
                                ref={mediaRef as React.RefObject<HTMLVideoElement>}
                                controls
                                className="question-media"
                            >
                                <source src={`${API_BASE_URL}${currentQuestion.mediaUrl}`}/>
                            </video>
                        )}
                    </div>
                )}

                {/* Question Text */}
                <h2 className="question-text">{currentQuestion.questionText}</h2>

                {/* Answer Buttons */}
                <div className="answers-grid">
                    {currentQuestion.answers.map((answer) => (
                        <button
                            key={answer.id}
                            onClick={() => handleAnswerClick(answer.id, answer.isCorrect)}
                            disabled={selectedAnswer !== null}
                            className={`answer-btn ${
                                selectedAnswer === answer.id
                                    ? answer.isCorrect
                                        ? 'correct'
                                        : 'incorrect'
                                    : ''
                            } ${
                                selectedAnswer !== null && answer.isCorrect ? 'show-correct' : ''
                            }`}
                        >
                            {answer.answerText}
                        </button>
                    ))}
                </div>
            </div>

            <button onClick={() => navigate('/categories')} className="back-btn">
                ← Back to Categories
            </button>
        </div>
    );
};

export default QuizPlayer;