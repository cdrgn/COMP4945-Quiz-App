import axios from 'axios';
import {AuthResponse, LoginRequest, Quiz, QuizDto, CreateQuizDto} from '../types';

// const API_BASE_URL = 'http://localhost:5291/api';   
const API_BASE_URL = 'https://trivia-quiz-app.westus3.cloudapp.azure.com/api';

// Create axios instance with default config
const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add token to requests automatically
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Auth API
export const authAPI = {
    login: async (credentials: LoginRequest): Promise<AuthResponse> => {
        const response = await api.post('/auth/login', credentials);
        return response.data;
    },

    register: async (credentials: LoginRequest & { role: string }): Promise<AuthResponse> => {
        const response = await api.post('/auth/register', credentials);
        return response.data;
    },
};

// Quiz API
export const quizAPI = {
    getCategories: async (): Promise<Quiz[]> => {
        const response = await api.get('/quiz/categories');
        return response.data;
    },

    getQuiz: async (id: number): Promise<QuizDto> => {
        const response = await api.get(`/quiz/${id}`);
        return response.data;
    },

    // Admin endpoints
    getAllQuizzes: async (): Promise<Quiz[]> => {
        const response = await api.get('/quiz/admin/all');
        return response.data;
    },

    createQuiz: async (quiz: any): Promise<Quiz> => { 
        const response = await api.post('/quiz/admin', quiz);
        return response.data;
    },

    updateQuiz: async (id: number, quiz: any): Promise<void> => { 
        await api.put(`/quiz/admin/${id}`, quiz);
    },

    deleteQuiz: async (id: number): Promise<void> => {
        await api.delete(`/quiz/admin/${id}`);
    },
};

// Media API
export const mediaAPI = {
    uploadFile: async (file: File): Promise<{ url: string; mediaType: string }> => {
        const formData = new FormData();
        formData.append('file', file);

        const response = await api.post('/media/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },
};

export default api;