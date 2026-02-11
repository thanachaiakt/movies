import { fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query';
import { Mutex } from 'async-mutex';
import { logout } from './authSlice';

const mutex = new Mutex();
const baseQuery = fetchBaseQuery({
    baseUrl: '/api',
    prepareHeaders: (headers) => {
        // Since we are using httpOnly cookies, we don't need to manually attach tokens
        return headers;
    },
});

export const baseQueryWithReauth: BaseQueryFn<
    string | FetchArgs,
    unknown,
    FetchBaseQueryError
> = async (args, api, extraOptions) => {
    // wait until the mutex is available without locking it
    await mutex.waitForUnlock();

    let result = await baseQuery(args, api, extraOptions);

    if (result.error && result.error.status === 401) {
        // prevent multiple calls to refresh endpoint
        if (!mutex.isLocked()) {
            const release = await mutex.acquire();
            try {
                // assume the refresh endpoint is at '/auth/refresh' relative to baseUrl '/api'
                // Wait, authApi uses /api/auth as baseUrl, movieApi uses /api
                // So baseQuery pointing to /api is correct for general use,
                // but we need to be careful about the refresh endpoint path.
                // The refresh endpoint is at /api/auth/refresh.
                // So we should call '/auth/refresh' here.

                const refreshResult = await baseQuery('/auth/refresh', api, extraOptions);

                if (refreshResult.data) {
                    // Retry the initial query
                    result = await baseQuery(args, api, extraOptions);
                } else {
                    api.dispatch(logout());
                }
            } finally {
                release();
            }
        } else {
            // wait until the mutex is available without locking it
            await mutex.waitForUnlock();
            result = await baseQuery(args, api, extraOptions);
        }
    }
    return result;
};
