# Movie Booking System - Frontend

React 19 + TypeScript + Vite + Redux Toolkit application for movie ticket booking.

## Architecture Overview

**Tech Stack**: React 19, Vite 8, Redux Toolkit (RTK Query), React Router 7, Tailwind CSS v4, TypeScript 5.9

**State Management Pattern**: 
- RTK Query APIs for server state ([src/store/authApi.ts](../src/store/authApi.ts), [src/store/movieApi.ts](../src/store/movieApi.ts))
- Redux slices for client state ([src/store/authSlice.ts](../src/store/authSlice.ts))
- Centralized store configuration in [src/store/store.ts](../src/store/store.ts)

**Authentication Flow**:
- httpOnly cookies for token storage (tokens never touch client JS)
- Custom `baseQueryWithReauth` handles 401s with automatic refresh ([src/store/baseQuery.ts](../src/store/baseQuery.ts))
- Mutex pattern prevents duplicate refresh requests during concurrent API calls
- `AuthInitializer` wrapper checks session on app load via `/api/auth/me` endpoint
- Manual auth state updates via `dispatch(setCredentials({ user }))` after login/register

## Critical Developer Workflows

**Development**: `npm run dev` - Starts Vite dev server on port 5173 with HMR
**Build**: `npm run build` - TypeScript compilation + Vite production build
**Lint**: `npm run lint` - ESLint with TypeScript, React Hooks, and React Refresh rules

**Backend Dependency**: Dev server proxies all `/api/*` requests to `http://localhost:5006` ([vite.config.ts](../vite.config.ts))

## Project-Specific Conventions

### API Integration Pattern
```typescript
// Define API with RTK Query createApi
export const movieApi = createApi({
  reducerPath: 'movieApi',
  baseQuery: baseQueryWithReauth,  // Use shared auth-aware base query
  tagTypes: ['Bookings', 'Movies'], // For cache invalidation
  endpoints: (builder) => ({...})
});

// Auto-generated hooks follow use[Endpoint][Query|Mutation] pattern
const { data, isLoading, error } = useGetMoviesQuery();
const [createBooking, { isLoading }] = useCreateBookingMutation();
```

### Authentication State Synchronization
Two-part auth state:
1. Server state via `useGetMeQuery()` in `AuthInitializer`
2. Client Redux state via `authSlice` (isAuthenticated flag + user object)

**Critical**: After successful login/register, dispatch `setCredentials({ user })` to sync client state

### Route Protection Pattern
```tsx
<Route path="/dashboard" element={
  <ProtectedRoute><DashboardPage /></ProtectedRoute>
} />
```
`ProtectedRoute` reads `state.auth.isAuthenticated` and redirects to `/login` with location state

### Styling System
**Tailwind CSS v4** with `@theme` directive (not `tailwind.config.js`)

Custom design tokens defined in [src/index.css](../src/index.css):
- Dark theme: `bg-bg` (#0f0d1a), `bg-surface` (#1e1b2e)
- Primary: `text-primary` (#6366f1), `bg-primary`, `border-primary`
- Accent: `text-accent` (#f472b6) for highlights
- Text: `text-text` (light), `text-text-muted` (muted)

**Glass morphism pattern**: `backdrop-blur-xl bg-surface/60 border border-white/10`

### TypeScript Patterns
- Separate interfaces per domain (User, Movie, Showtime, Booking)
- Request/Response type pairs (e.g., `LoginRequest`, `AuthResponse`)
- Redux types exported: `RootState`, `AppDispatch` from [store.ts](../src/store/store.ts)

## Integration Points

**Backend API Endpoints** (via proxy):
- Auth: `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, `/api/auth/logout`, `/api/auth/me`
- Movies: `/api/movies`, `/api/movies/:id`
- Bookings: `/api/bookings` (POST/GET), `/api/bookings/:id/cancel` (PUT)

**Cache Invalidation Strategy**:
- Creating booking invalidates `['Bookings', 'Movies']` tags (updates seat availability)
- Canceling booking invalidates `['Bookings']` only
- See [movieApi.ts](../src/store/movieApi.ts) `invalidatesTags` configuration

## Common Gotchas

1. **Auth refresh race condition**: `baseQueryWithReauth` uses `async-mutex` to ensure single refresh call—don't modify without understanding mutex pattern
2. **Redirect loops**: `LoginPage` guards against `location.state.from === '/login'` infinite loops
3. **Manual dispatch required**: RTK Query doesn't auto-update Redux slices—must dispatch `setCredentials` after auth mutations
4. **Tailwind v4 syntax**: Use `@theme` directive, not config file. Import `"tailwindcss"` in CSS, not `@tailwind` directives
5. **Proxy limitations**: API proxy only works in dev mode—production needs backend on same origin or CORS configuration

## Key Files for Onboarding

- [src/store/baseQuery.ts](../src/store/baseQuery.ts) - Auth refresh logic (mutex pattern)
- [src/components/AuthInitializer.tsx](../src/components/AuthInitializer.tsx) - Session bootstrap flow
- [src/store/store.ts](../src/store/store.ts) - Redux store configuration
- [vite.config.ts](../vite.config.ts) - Build config + dev proxy
- [src/index.css](../src/index.css) - Design system tokens
