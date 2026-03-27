import React,  {
    useState, useEffect
}
from 'react';
import {
    useNavigate
}
from 'react-router-dom';
import {
    quizAPI
}
from '../services/api';
import {
    Quiz
}
from '../types';
import './Categories.css';

const Categories : React.FC = () =>
{
    const [quizzes, setQuizzes] = useState<Quiz[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');
    const navigate  = useNavigate();

    useEffect(() => { loadQuizzes(); }, []);

    const loadQuizzes  = async () =>
    {
        try
        {
            const data  = await quizAPI.getCategories();
            setQuizzes(data);
        }
        catch (err:

        any) {
            setError('Failed to load quizzes. Please try again.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleQuizClick  = (quizId: number) => {
        navigate(` / quiz /${
            quizId
        }`);
    }
    ;

    if (isLoading)
    {
        return <div className = "loading" > Loading quizzes...</div >;
    }

    if (error)
    {
        return <div className = "error" >{
            error
        }</div >;
    }

    return (
        <div className = "categories-container" >
        <div className = "categories-header" >
        <h1 > Choose Your Quiz</
    h1 >
        <p > Test your knowledge with our trivia categories</
    p >
        </div >

        <div className = "quiz-grid" >
    {
        quizzes.map((quiz) => (
            < div
        key = {
            quiz.id
        }
        className = "quiz-card"
        onClick = {
            () => handleQuizClick(quiz.id)
        }
        >
        <div className = "quiz-icon" >🎯</div >
            <h2 >{
            quiz.categoryName
        }</h2 >
            <p >{
            quiz.title
        }</p >
            <div className = "quiz-info" >
                <span >{ quiz.questionCount || 0} Questions </span >
            </div >
            <button className = "play-btn" > Play Now </button >
            </div >
            ))
    }
    </div >

    {
        quizzes.length === 0 && (
            <div className = "no-quizzes" >
            <p > No quizzes available yet.</p >
            <p > Check back soon!</p >
            </div >
            )
    }
    </div >
        );
};

export default Categories;