import React, {useState, useEffect} from 'react';
import {BrowserRouter as Router, Routes, Route, Navigate} from 'react-router-dom';
import Login from './components/Login';
import Categories from './components/Categories';
import QuizPlayer from './components/QuizPlayer';
import AdminPanel from './components/AdminPanel';
import HostScreen from './components/HostScreen';             // ← NEW
import ParticipantScreen from './components/ParticipantScreen'; // ← NEW
import './App.css';

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [userRole, setUserRole] = useState<string>('');
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        // Check if user is already logged in
        const token = localStorage.getItem('token');
        const role = localStorage.getItem('role');
        if (token && role) {
            setIsAuthenticated(true);
            setUserRole(role);
        }
        setIsLoading(false);
    }, []);

    const handleLogin = (token: string, email: string, role: string) => {
        localStorage.setItem('token', token);
        localStorage.setItem('email', email);
        localStorage.setItem('role', role);
        setIsAuthenticated(true);
        setUserRole(role);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('email');
        localStorage.removeItem('role');
        setIsAuthenticated(false);
        setUserRole('');
    };

    if (isLoading) {
        return <div className="loading">Loading...</div>;
    }

    return (
        <Router>
            <div className="App">
                {isAuthenticated && (
                    <nav className="navbar">
                        <h1>🎯 Trivia Quiz</h1>
                        <div className="nav-links">
                            <a href="/categories">Quizzes</a>
                            {/* ── NEW: Moderated mode links ── */}
                            {userRole === 'Admin' && <a href="/host">Host Quiz</a>}
                            <a href="/join">Join Quiz</a>
                            {userRole === 'Admin' && <a href="/admin">Admin Panel</a>}
                            <button onClick={handleLogout} className="logout-btn">Logout</button>
                        </div>
                    </nav>
                )}

                <Routes>
                    <Route
                        path="/login"
                        element={
                            isAuthenticated ? (
                                <Navigate to="/categories"/>
                            ) : (
                                <Login onLogin={handleLogin}/>
                            )
                        }
                    />
                    <Route
                        path="/categories"
                        element={
                            isAuthenticated ? (
                                <Categories/>
                            ) : (
                                <Navigate to="/login"/>
                            )
                        }
                    />
                    <Route
                        path="/quiz/:id"
                        element={
                            isAuthenticated ? (
                                <QuizPlayer/>
                            ) : (
                                <Navigate to="/login"/>
                            )
                        }
                    />
                    <Route
                        path="/admin"
                        element={
                            isAuthenticated && userRole === 'Admin' ? (
                                <AdminPanel/>
                            ) : (
                                <Navigate to="/categories"/>
                            )
                        }
                    />

                    {/* ── NEW: Part B — Moderated mode routes ── */}
                    <Route
                        path="/host"
                        element={
                            isAuthenticated && userRole === 'Admin' ? (
                                <HostScreen/>
                            ) : (
                                <Navigate to="/login"/>
                            )
                        }
                    />
                    <Route
                        path="/join"
                        element={
                            isAuthenticated ? (
                                <ParticipantScreen/>
                            ) : (
                                <Navigate to="/login"/>
                            )
                        }
                    />

                    <Route path="/" element={<Navigate to="/login"/>}/>
                </Routes>
            </div>
        </Router>
    );
}

export default App;
