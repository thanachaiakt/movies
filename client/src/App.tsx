import { Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import MoviesPage from './pages/MoviesPage';
import MovieDetailPage from './pages/MovieDetailPage';
import MyBookingsPage from './pages/MyBookingsPage';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar';
import ChatWidget from './components/ChatWidget';
import { useAppSelector } from './store/store';

import AuthInitializer from './components/AuthInitializer';

function App() {
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);

  return (
    <AuthInitializer>
      <Navbar />

      <Routes>
        <Route
          path="/dashboard"
          index
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/my-bookings"
          element={
            <ProtectedRoute>
              <MyBookingsPage />
            </ProtectedRoute>
          }
        />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/movies" element={<MoviesPage />} />
        <Route path="/movies/:id" element={<MovieDetailPage />} />

        <Route path="*" element={<Navigate to="/movies" replace />} />
      </Routes>

      {/* Chat Widget - only show when authenticated */}
      {isAuthenticated && <ChatWidget />}
    </AuthInitializer>
  );
}

export default App;
