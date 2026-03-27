import React, {useState, useEffect} from 'react';
import {quizAPI, mediaAPI} from '../services/api';
import {Quiz} from '../types';
import './AdminPanel.css';

const AdminPanel: React.FC = () => {
    const [quizzes, setQuizzes] = useState<Quiz[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [editingQuiz, setEditingQuiz] = useState<Quiz | null>(null);

    // Form state
    const [categoryName, setCategoryName] = useState('');
    const [title, setTitle] = useState('');
    const [questions, setQuestions] = useState<any[]>([
        {
            questionText: '', mediaUrl: '', mediaType: '', answers: [
                {answerText: '', isCorrect: false},
                {answerText: '', isCorrect: false}
            ]
        }
    ]);

    useEffect(() => {
        loadQuizzes();
    }, []);

    const loadQuizzes = async () => {
        try {
            const data = await quizAPI.getAllQuizzes();
            setQuizzes(data);
        } catch (err) {
            alert('Failed to load quizzes');
        } finally {
            setIsLoading(false);
        }
    };

    const handleEditQuiz = async (id: number) => {
        try {
            // Fetch the full quiz with questions
            const quizData = await quizAPI.getQuiz(id);

            // Populate form with existing data
            setCategoryName(quizData.categoryName);
            setTitle(quizData.title);
            setQuestions(quizData.questions.map(q => ({
                questionText: q.questionText,
                mediaUrl: q.mediaUrl || '',
                mediaType: q.mediaType || '',
                answers: q.answers.map(a => ({
                    answerText: a.answerText,
                    isCorrect: a.isCorrect,
                    orderIndex: a.orderIndex
                }))
            })));

            // Show form in edit mode
            setEditingQuiz({id: quizData.id} as any);
            setShowCreateForm(true);
        } catch (err) {
            alert('Failed to load quiz for editing');
        }
    };

    const handleDeleteQuiz = async (id: number) => {
        if (!window.confirm('Are you sure you want to delete this quiz?')) return;

        try {
            await quizAPI.deleteQuiz(id);
            setQuizzes(quizzes.filter(q => q.id !== id));
            alert('Quiz deleted successfully!');
        } catch (err) {
            alert('Failed to delete quiz');
        }
    };

    const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>, questionIndex: number) => {
        const file = e.target.files?.[0];
        if (!file) return;

        try {
            const response = await mediaAPI.uploadFile(file);
            const newQuestions = [...questions];
            newQuestions[questionIndex].mediaUrl = response.url;
            newQuestions[questionIndex].mediaType = response.mediaType;
            setQuestions(newQuestions);
            alert('File uploaded successfully!');
        } catch (err) {
            alert('Failed to upload file');
        }
    };

    const addQuestion = () => {
        setQuestions([...questions, {
            questionText: '',
            mediaUrl: '',
            mediaType: '',
            answers: [
                {answerText: '', isCorrect: false},
                {answerText: '', isCorrect: false}
            ]
        }]);
    };

    const removeQuestion = (index: number) => {
        setQuestions(questions.filter((_, i) => i !== index));
    };

    const addAnswer = (questionIndex: number) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers.push({answerText: '', isCorrect: false});
        setQuestions(newQuestions);
    };

    const removeAnswer = (questionIndex: number, answerIndex: number) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers = newQuestions[questionIndex].answers.filter(
            (_: any, i: number) => i !== answerIndex
        );
        setQuestions(newQuestions);
    };

    const updateQuestion = (index: number, field: string, value: any) => {
        const newQuestions = [...questions];
        newQuestions[index][field] = value;
        setQuestions(newQuestions);
    };

    const updateAnswer = (questionIndex: number, answerIndex: number, field: string, value: any) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers[answerIndex][field] = value;

        // If setting this answer as correct, uncheck others
        if (field === 'isCorrect' && value === true) {
            newQuestions[questionIndex].answers.forEach((a: any, i: number) => {
                if (i !== answerIndex) a.isCorrect = false;
            });
        }

        setQuestions(newQuestions);
    };

    const handleSubmitQuiz = async (e: React.FormEvent) => {
        e.preventDefault();

        // Validation
        if (!categoryName || !title) {
            alert('Please fill in category name and title');
            return;
        }

        if (questions.length === 0) {
            alert('Please add at least one question');
            return;
        }

        for (let i = 0; i < questions.length; i++) {
            const q = questions[i];
            if (!q.questionText) {
                alert(`Question ${i + 1} is missing text`);
                return;
            }
            if (q.answers.length < 2) {
                alert(`Question ${i + 1} needs at least 2 answers`);
                return;
            }
            if (!q.answers.some((a: any) => a.isCorrect)) {
                alert(`Question ${i + 1} needs a correct answer`);
                return;
            }
        }

        const quizData = {
            categoryName,
            title,
            isActive: true,
            questions: questions.map((q, i) => ({
                questionText: q.questionText,
                mediaUrl: q.mediaUrl || null,
                mediaType: q.mediaType || null,
                orderIndex: i + 1,
                answers: q.answers.map((a: any, j: number) => ({
                    answerText: a.answerText,
                    isCorrect: a.isCorrect,
                    orderIndex: j + 1
                }))
            }))
        };

        try {
            if (editingQuiz) {
                await quizAPI.updateQuiz(editingQuiz.id, quizData);
                alert('Quiz updated successfully!');
            } else {
                await quizAPI.createQuiz(quizData);
                alert('Quiz created successfully!');
            }

            resetForm();
            loadQuizzes();
        } catch (err: any) {
            //alert('Failed to save quiz: ' + (err.response?.data || err.message));
            console.error('Full error:', err);
            console.error('Error response:', err.response);
            console.error('Error data:', err.response?.data);
            alert('Failed to save quiz. Check console for details.');
        }
    };

    const resetForm = () => {
        setCategoryName('');
        setTitle('');
        setQuestions([{
            questionText: '',
            mediaUrl: '',
            mediaType: '',
            answers: [
                {answerText: '', isCorrect: false},
                {answerText: '', isCorrect: false}
            ]
        }]);
        setShowCreateForm(false);
        setEditingQuiz(null);
    };

    if (isLoading) {
        return <div className="loading">Loading admin panel...</div>;
    }

    return (
        <div className="admin-container">
            <div className="admin-header">
                <h1>Admin Panel</h1>
                <button
                    onClick={() => setShowCreateForm(!showCreateForm)}
                    className="create-btn"
                >
                    {showCreateForm ? 'Cancel' : '+ Create New Quiz'}
                </button>
            </div>

            {showCreateForm && (
                <div className="quiz-form-container">
                    <h2>{editingQuiz ? 'Edit Quiz' : 'Create New Quiz'}</h2>
                    <form onSubmit={handleSubmitQuiz}>
                        <div className="form-group">
                            <label>Category Name *</label>
                            <input
                                type="text"
                                value={categoryName}
                                onChange={(e) => setCategoryName(e.target.value)}
                                placeholder="e.g., 1980s Music"
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label>Quiz Title *</label>
                            <input
                                type="text"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                placeholder="e.g., Best Hits of the 80s"
                                required
                            />
                        </div>

                        <div className="questions-section">
                            <h3>Questions</h3>
                            {questions.map((question, qIndex) => (
                                <div key={qIndex} className="question-block">
                                    <div className="question-header">
                                        <h4>Question {qIndex + 1}</h4>
                                        {questions.length > 1 && (
                                            <button
                                                type="button"
                                                onClick={() => removeQuestion(qIndex)}
                                                className="remove-btn"
                                            >
                                                Remove
                                            </button>
                                        )}
                                    </div>

                                    <div className="form-group">
                                        <label>Question Text *</label>
                                        <input
                                            type="text"
                                            value={question.questionText}
                                            onChange={(e) => updateQuestion(qIndex, 'questionText', e.target.value)}
                                            placeholder="Enter your question"
                                            required
                                        />
                                    </div>

                                    <div className="form-group">
                                        <label>Media File (optional)</label>
                                        <input
                                            type="file"
                                            accept="image/*,audio/*,video/*"
                                            onChange={(e) => handleFileUpload(e, qIndex)}
                                            className="file-input"
                                        />
                                        {question.mediaUrl && (
                                            <div className="media-preview">
                                                ✓ File uploaded: {question.mediaType}
                                            </div>
                                        )}
                                    </div>

                                    <div className="answers-section">
                                        <label>Answers (minimum 2) *</label>
                                        {question.answers.map((answer: any, aIndex: number) => (
                                            <div key={aIndex} className="answer-row">
                                                <input
                                                    type="text"
                                                    value={answer.answerText}
                                                    onChange={(e) => updateAnswer(qIndex, aIndex, 'answerText', e.target.value)}
                                                    placeholder={`Answer ${aIndex + 1}`}
                                                    required
                                                />
                                                <label className="correct-checkbox">
                                                    <input
                                                        type="checkbox"
                                                        checked={answer.isCorrect}
                                                        onChange={(e) => updateAnswer(qIndex, aIndex, 'isCorrect', e.target.checked)}
                                                    />
                                                    Correct
                                                </label>
                                                {question.answers.length > 2 && (
                                                    <button
                                                        type="button"
                                                        onClick={() => removeAnswer(qIndex, aIndex)}
                                                        className="remove-answer-btn"
                                                    >
                                                        ✕
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                        {question.answers.length < 4 && (
                                            <button
                                                type="button"
                                                onClick={() => addAnswer(qIndex)}
                                                className="add-answer-btn"
                                            >
                                                + Add Answer
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))}

                            <button type="button" onClick={addQuestion} className="add-question-btn">
                                + Add Question
                            </button>
                        </div>

                        <div className="form-actions">
                            <button type="submit" className="submit-btn">
                                {editingQuiz ? 'Update Quiz' : 'Create Quiz'}
                            </button>
                            <button type="button" onClick={resetForm} className="cancel-btn">
                                Cancel
                            </button>
                        </div>
                    </form>
                </div>
            )}

            <div className="quizzes-list">
                <h2>Existing Quizzes</h2>
                {quizzes.length === 0 ? (
                    <p className="no-quizzes">No quizzes yet. Create your first one!</p>
                ) : (
                    <div className="quiz-cards">
                        {quizzes.map((quiz) => (
                            <div key={quiz.id} className="quiz-item">
                                <div className="quiz-item-header">
                                    <h3>{quiz.categoryName}</h3>
                                    <div className="quiz-actions">
                                        <button
                                            onClick={() => handleEditQuiz(quiz.id)}
                                            className="edit-btn"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            onClick={() => handleDeleteQuiz(quiz.id)}
                                            className="delete-btn"
                                        >
                                            Delete
                                        </button>
                                    </div>
                                </div>
                                <p>{quiz.title}</p>
                                <div className="quiz-meta">
                                    <span>{quiz.questions?.length || 0} questions</span>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default AdminPanel;