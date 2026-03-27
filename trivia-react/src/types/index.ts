// Type definitions matching backend DTOs

export interface User {
    email: string;
    role: string;
    token: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    email: string;
    role: string;
}

export interface Quiz {
    id: number;
    categoryName: string;
    title: string;
    questionCount?: number;
    questions?: Question[];
}

export interface Question {
    id: number;
    questionText: string;
    mediaUrl?: string;
    mediaType?: string;
    answers: Answer[];
}

export interface Answer {
    id: number;
    answerText: string;
    isCorrect: boolean;
}

export interface QuizDto {
    id: number;
    categoryName: string;
    title: string;
    questions: Question[];
}

// For creating new quizzes (without IDs)
export interface CreateQuestionDto {
    questionText: string;
    mediaUrl?: string | null;
    mediaType?: string | null;
    orderIndex: number;
    answers: CreateAnswerDto[];
}

export interface CreateAnswerDto {
    answerText: string;
    isCorrect: boolean;
    orderIndex: number;
}

export interface CreateQuizDto {
    categoryName: string;
    title: string;
    isActive: boolean;
    questions: CreateQuestionDto[];
}