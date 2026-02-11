import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store/store';
import { useGetMovieQuery, useCreateBookingMutation } from '../store/movieApi';
import type { Showtime } from '../store/movieApi';

export default function MovieDetailPage() {
    const { id } = useParams<{ id: string }>();
    const { isAuthenticated } = useSelector((state: RootState) => state.auth);
    const { data: movie, isLoading } = useGetMovieQuery(Number(id));
    const [createBooking, { isLoading: isBooking }] = useCreateBookingMutation();

    const [selectedShowtime, setSelectedShowtime] = useState<Showtime | null>(null);
    const [seats, setSeats] = useState(1);
    const [success, setSuccess] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleBook = async () => {
        if (!selectedShowtime) return;
        setError(null);
        try {
            const result = await createBooking({
                showtimeId: selectedShowtime.id,
                seatsBooked: seats,
            }).unwrap();
            setSuccess(`Booked! Code: ${result.bookingCode}`);
            setSelectedShowtime(null);
        } catch (err: any) {
            setError(err?.data?.message || 'Booking failed');
        }
    };

    if (isLoading) return (
        <div style={{ minHeight: '100vh', background: '#0a0618', paddingTop: 96, textAlign: 'center' }}>
            <div style={{
                width: 40, height: 40,
                border: '3px solid rgba(139,92,246,0.2)',
                borderTopColor: '#8b5cf6',
                borderRadius: '50%',
                animation: 'spin 0.8s linear infinite',
                margin: '80px auto',
            }} />
            <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
        </div>
    );

    if (!movie) return (
        <div style={{ minHeight: '100vh', background: '#0a0618', paddingTop: 96, textAlign: 'center', color: '#fff' }}>
            <p>Movie not found</p>
            <Link to="/movies" style={{ color: '#a78bfa' }}>← Back to movies</Link>
        </div>
    );

    const groupedByDate: Record<string, Showtime[]> = {};
    movie.showtimes.forEach(s => {
        const date = new Date(s.startTime).toLocaleDateString('en-US', {
            weekday: 'short', month: 'short', day: 'numeric',
        });
        if (!groupedByDate[date]) groupedByDate[date] = [];
        groupedByDate[date].push(s);
    });

    return (
        <div style={{ minHeight: '100vh', background: '#0a0618', paddingTop: 80 }}>
            {/* Hero */}
            <div style={{
                position: 'relative',
                padding: '40px 24px',
                maxWidth: 1200,
                margin: '0 auto',
                display: 'flex',
                gap: 40,
                flexWrap: 'wrap',
            }}>
                <div style={{
                    width: 280,
                    flexShrink: 0,
                    borderRadius: 16,
                    overflow: 'hidden',
                    boxShadow: '0 20px 60px rgba(0,0,0,0.5)',
                }}>
                    <img
                        src={movie.posterUrl}
                        alt={movie.title}
                        style={{ width: '100%', display: 'block' }}
                        onError={e => { (e.target as HTMLImageElement).style.display = 'none'; }}
                    />
                </div>

                <div style={{ flex: 1, minWidth: 300 }}>
                    <h1 style={{
                        fontSize: 36,
                        fontWeight: 800,
                        color: '#fff',
                        margin: 0,
                        lineHeight: 1.2,
                    }}>
                        {movie.title}
                    </h1>

                    <div style={{
                        display: 'flex',
                        gap: 12,
                        marginTop: 16,
                        flexWrap: 'wrap',
                    }}>
                        {[movie.genre, movie.rating, `${movie.durationMinutes} min`].map((tag, i) => (
                            <span key={i} style={{
                                padding: '4px 12px',
                                borderRadius: 6,
                                background: i === 0 ? 'rgba(139,92,246,0.15)' : 'rgba(255,255,255,0.06)',
                                color: i === 0 ? '#a78bfa' : 'rgba(255,255,255,0.6)',
                                fontSize: 13,
                                fontWeight: 500,
                            }}>
                                {tag}
                            </span>
                        ))}
                    </div>

                    <p style={{
                        color: 'rgba(255,255,255,0.6)',
                        fontSize: 15,
                        lineHeight: 1.7,
                        marginTop: 20,
                    }}>
                        {movie.description}
                    </p>

                    {success && (
                        <div style={{
                            marginTop: 20,
                            padding: '16px 20px',
                            borderRadius: 12,
                            background: 'rgba(34,197,94,0.1)',
                            border: '1px solid rgba(34,197,94,0.2)',
                            color: '#22c55e',
                            fontSize: 14,
                        }}>
                            ✅ {success}
                            <Link to="/my-bookings" style={{
                                display: 'block',
                                marginTop: 8,
                                color: '#a78bfa',
                                fontSize: 13,
                            }}>
                                View my bookings →
                            </Link>
                        </div>
                    )}
                </div>
            </div>

            {/* Showtimes */}
            <div style={{ maxWidth: 1200, margin: '0 auto', padding: '0 24px 80px' }}>
                <h2 style={{
                    fontSize: 24,
                    fontWeight: 700,
                    color: '#fff',
                    marginBottom: 24,
                }}>
                    Showtimes
                </h2>

                {Object.keys(groupedByDate).length === 0 && (
                    <p style={{ color: 'rgba(255,255,255,0.4)' }}>No upcoming showtimes available.</p>
                )}

                {Object.entries(groupedByDate).map(([date, showtimes]) => (
                    <div key={date} style={{ marginBottom: 28 }}>
                        <h3 style={{
                            fontSize: 14,
                            fontWeight: 600,
                            color: 'rgba(255,255,255,0.4)',
                            textTransform: 'uppercase',
                            letterSpacing: 1,
                            marginBottom: 12,
                        }}>
                            {date}
                        </h3>
                        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                            {showtimes.map(st => (
                                <button
                                    key={st.id}
                                    onClick={() => {
                                        if (!isAuthenticated) return;
                                        setSelectedShowtime(st);
                                        setSeats(1);
                                        setSuccess(null);
                                        setError(null);
                                    }}
                                    style={{
                                        padding: '12px 20px',
                                        borderRadius: 10,
                                        border: selectedShowtime?.id === st.id
                                            ? '1px solid #8b5cf6'
                                            : '1px solid rgba(255,255,255,0.08)',
                                        background: selectedShowtime?.id === st.id
                                            ? 'rgba(139,92,246,0.15)'
                                            : 'rgba(255,255,255,0.03)',
                                        color: '#fff',
                                        cursor: isAuthenticated ? 'pointer' : 'not-allowed',
                                        transition: 'all 0.2s',
                                        textAlign: 'left',
                                        opacity: isAuthenticated ? 1 : 0.5,
                                    }}
                                >
                                    <div style={{ fontSize: 16, fontWeight: 600 }}>
                                        {new Date(st.startTime).toLocaleTimeString('en-US', {
                                            hour: '2-digit', minute: '2-digit',
                                        })}
                                    </div>
                                    <div style={{ fontSize: 12, color: 'rgba(255,255,255,0.4)', marginTop: 4 }}>
                                        {st.theater} · ฿{st.price} · {st.availableSeats} seats
                                    </div>
                                </button>
                            ))}
                        </div>
                    </div>
                ))}

                {!isAuthenticated && (
                    <p style={{ color: 'rgba(255,255,255,0.4)', fontSize: 14, marginTop: 8 }}>
                        <Link to="/login" style={{ color: '#a78bfa' }}>Login</Link> to book tickets
                    </p>
                )}

                {/* Booking Panel */}
                {selectedShowtime && (
                    <div style={{
                        marginTop: 32,
                        padding: 24,
                        borderRadius: 16,
                        background: 'rgba(139,92,246,0.06)',
                        border: '1px solid rgba(139,92,246,0.15)',
                    }}>
                        <h3 style={{ color: '#fff', margin: 0, fontSize: 18, fontWeight: 600 }}>
                            Book Tickets
                        </h3>
                        <p style={{ color: 'rgba(255,255,255,0.5)', fontSize: 14, margin: '8px 0 20px' }}>
                            {new Date(selectedShowtime.startTime).toLocaleString('en-US', {
                                weekday: 'short', month: 'short', day: 'numeric',
                                hour: '2-digit', minute: '2-digit',
                            })}
                            {' · '}{selectedShowtime.theater}
                        </p>

                        <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 20 }}>
                            <label style={{ color: 'rgba(255,255,255,0.6)', fontSize: 14 }}>Seats:</label>
                            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                <button
                                    onClick={() => setSeats(Math.max(1, seats - 1))}
                                    style={counterBtnStyle}
                                >–</button>
                                <span style={{ color: '#fff', fontSize: 18, fontWeight: 700, width: 32, textAlign: 'center' }}>
                                    {seats}
                                </span>
                                <button
                                    onClick={() => setSeats(Math.min(10, selectedShowtime.availableSeats, seats + 1))}
                                    style={counterBtnStyle}
                                >+</button>
                            </div>
                            <span style={{ color: 'rgba(255,255,255,0.3)', fontSize: 13 }}>
                                ({selectedShowtime.availableSeats} available)
                            </span>
                        </div>

                        <div style={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                        }}>
                            <div style={{ color: '#fff', fontSize: 22, fontWeight: 700 }}>
                                ฿{(selectedShowtime.price * seats).toLocaleString()}
                            </div>
                            <button
                                onClick={handleBook}
                                disabled={isBooking}
                                style={{
                                    padding: '12px 32px',
                                    borderRadius: 10,
                                    border: 'none',
                                    background: 'linear-gradient(135deg, #8b5cf6, #6d28d9)',
                                    color: '#fff',
                                    fontSize: 15,
                                    fontWeight: 600,
                                    cursor: isBooking ? 'not-allowed' : 'pointer',
                                    opacity: isBooking ? 0.7 : 1,
                                }}
                            >
                                {isBooking ? 'Booking...' : 'Confirm Booking'}
                            </button>
                        </div>

                        {error && (
                            <p style={{ color: '#ef4444', fontSize: 14, marginTop: 12 }}>
                                ❌ {error}
                            </p>
                        )}
                    </div>
                )}
            </div>

            <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
        </div>
    );
}

const counterBtnStyle: React.CSSProperties = {
    width: 36, height: 36,
    borderRadius: 8,
    border: '1px solid rgba(255,255,255,0.1)',
    background: 'rgba(255,255,255,0.05)',
    color: '#fff',
    fontSize: 18,
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
};
