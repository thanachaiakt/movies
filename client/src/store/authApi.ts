import { createApi } from '@reduxjs/toolkit/query/react';

export interface User {
    id?: string;
    email: string;
    fullName: string;
}

export interface AuthResponse {
    email: string;
    fullName: string;
}

export interface RegisterRequest {
    email: string;
    password: string;
    confirmPassword: string;
    fullName: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

import { baseQueryWithReauth } from './baseQuery';

export const authApi = createApi({
    reducerPath: 'authApi',
    baseQuery: baseQueryWithReauth,
    endpoints: (builder) => ({
        register: builder.mutation<AuthResponse, RegisterRequest>({
            query: (body) => ({
                url: '/auth/register',
                method: 'POST',
                body,
            }),
        }),
        login: builder.mutation<AuthResponse, LoginRequest>({
            query: (body) => ({
                url: '/auth/login',
                method: 'POST',
                body,
            }),
        }),
        refreshToken: builder.mutation<AuthResponse, void>({
            query: () => ({
                url: '/auth/refresh',
                method: 'POST',
            }),
        }),
        logout: builder.mutation<void, void>({
            query: () => ({
                url: '/auth/logout',
                method: 'POST',
            }),
        }),
        getMe: builder.query<User, void>({
            query: () => '/auth/me',
        }),
    }),
});

export const {
    useRegisterMutation,
    useLoginMutation,
    useRefreshTokenMutation,
    useLogoutMutation,
    useGetMeQuery,
} = authApi;
