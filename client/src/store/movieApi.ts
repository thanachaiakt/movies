import { createApi } from '@reduxjs/toolkit/query/react';

export interface Movie {
    id: number;
    title: string;
    description: string;
    posterUrl: string;
    genre: string;
    durationMinutes: number;
    rating: string;
    releaseDate: string;
}

export interface Showtime {
    id: number;
    movieId: number;
    movieTitle: string;
    startTime: string;
    theater: string;
    price: number;
    totalSeats: number;
    availableSeats: number;
}

export interface MovieDetail extends Movie {
    showtimes: Showtime[];
}

export interface BookingResponse {
    id: number;
    bookingCode: string;
    movieTitle: string;
    theater: string;
    showTime: string;
    seatsBooked: number;
    totalPrice: number;
    status: string;
    createdAt: string;
}

export interface CreateBookingRequest {
    showtimeId: number;
    seatsBooked: number;
}

import { baseQueryWithReauth } from './baseQuery';

export const movieApi = createApi({
    reducerPath: 'movieApi',
    baseQuery: baseQueryWithReauth,
    tagTypes: ['Bookings', 'Movies'],
    endpoints: (builder) => ({
        getMovies: builder.query<Movie[], void>({
            query: () => '/movies',
            providesTags: ['Movies'],
        }),
        getMovie: builder.query<MovieDetail, number>({
            query: (id) => `/movies/${id}`,
            providesTags: (_result, _error, id) => [{ type: 'Movies', id }],
        }),
        createBooking: builder.mutation<BookingResponse, CreateBookingRequest>({
            query: (body) => ({
                url: '/bookings',
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Bookings', 'Movies'],
        }),
        getMyBookings: builder.query<BookingResponse[], void>({
            query: () => '/bookings',
            providesTags: ['Bookings'],
        }),
        cancelBooking: builder.mutation<{ message: string }, number>({
            query: (id) => ({
                url: `/bookings/${id}/cancel`,
                method: 'PUT',
            }),
            invalidatesTags: ['Bookings'],
        }),
    }),
});

export const {
    useGetMoviesQuery,
    useGetMovieQuery,
    useCreateBookingMutation,
    useGetMyBookingsQuery,
    useCancelBookingMutation,
} = movieApi;
