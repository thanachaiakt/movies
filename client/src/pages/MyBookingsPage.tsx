import { useGetMyBookingsQuery, useCancelBookingMutation } from '../store/movieApi';
import { Link } from 'react-router-dom';

export default function MyBookingsPage() {
    const { data: bookings, isLoading } = useGetMyBookingsQuery();
    const [cancelBooking] = useCancelBookingMutation();

    const handleCancel = async (id: number) => {
        if (!confirm('ต้องการยกเลิกการจองนี้?')) return;
        await cancelBooking(id);
    };

    return (
        <div style={{ minHeight: '100vh', background: '#0a0618', paddingTop: 96 }}>
            <div style={{ maxWidth: 900, margin: '0 auto', padding: '0 24px 64px' }}>
                <h1 style={{
                    fontSize: 32,
                    fontWeight: 800,
                    background: 'linear-gradient(135deg, #fff, #a78bfa)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    margin: '0 0 32px',
                }}>
                    My Bookings
                </h1>

                {isLoading && (
                    <div style={{ textAlign: 'center', padding: 80 }}>
                        <div style={{
                            width: 40, height: 40,
                            border: '3px solid rgba(139,92,246,0.2)',
                            borderTopColor: '#8b5cf6',
                            borderRadius: '50%',
                            animation: 'spin 0.8s linear infinite',
                            margin: '0 auto',
                        }} />
                    </div>
                )}

                {!isLoading && (!bookings || bookings.length === 0) && (
                    <div style={{
                        textAlign: 'center',
                        padding: 60,
                        borderRadius: 16,
                        background: 'rgba(255,255,255,0.02)',
                        border: '1px solid rgba(255,255,255,0.06)',
                    }}>
                        <p style={{ color: 'rgba(255,255,255,0.4)', fontSize: 16 }}>
                            No bookings yet
                        </p>
                        <Link to="/movies" style={{
                            display: 'inline-block',
                            marginTop: 16,
                            padding: '10px 24px',
                            borderRadius: 8,
                            background: 'linear-gradient(135deg, #8b5cf6, #6d28d9)',
                            color: '#fff',
                            textDecoration: 'none',
                            fontSize: 14,
                            fontWeight: 600,
                        }}>
                            Browse Movies
                        </Link>
                    </div>
                )}

                <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                    {bookings?.map(booking => {
                        const isCancelled = booking.status === 'Cancelled';
                        const isPast = new Date(booking.showTime) < new Date();

                        return (
                            <div key={booking.id} style={{
                                padding: 24,
                                borderRadius: 16,
                                background: 'rgba(255,255,255,0.03)',
                                border: `1px solid ${isCancelled ? 'rgba(239,68,68,0.15)' : 'rgba(255,255,255,0.06)'}`,
                                opacity: isCancelled ? 0.6 : 1,
                                transition: 'all 0.2s',
                            }}>
                                <div style={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'flex-start',
                                    flexWrap: 'wrap',
                                    gap: 16,
                                }}>
                                    <div>
                                        <h3 style={{ margin: 0, color: '#fff', fontSize: 18, fontWeight: 700 }}>
                                            {booking.movieTitle}
                                        </h3>
                                        <div style={{
                                            display: 'flex',
                                            gap: 16,
                                            marginTop: 10,
                                            fontSize: 13,
                                            color: 'rgba(255,255,255,0.45)',
                                            flexWrap: 'wrap',
                                        }}>
                                            <span>📅 {new Date(booking.showTime).toLocaleString('en-US', {
                                                weekday: 'short', month: 'short', day: 'numeric',
                                                hour: '2-digit', minute: '2-digit',
                                            })}</span>
                                            <span>🏛️ {booking.theater}</span>
                                            <span>💺 {booking.seatsBooked} seat{booking.seatsBooked > 1 ? 's' : ''}</span>
                                            <span>💰 ฿{booking.totalPrice.toLocaleString()}</span>
                                        </div>
                                    </div>

                                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                                        <span style={{
                                            padding: '4px 12px',
                                            borderRadius: 6,
                                            fontSize: 12,
                                            fontWeight: 600,
                                            background: isCancelled
                                                ? 'rgba(239,68,68,0.15)'
                                                : isPast
                                                    ? 'rgba(255,255,255,0.06)'
                                                    : 'rgba(34,197,94,0.15)',
                                            color: isCancelled
                                                ? '#ef4444'
                                                : isPast
                                                    ? 'rgba(255,255,255,0.4)'
                                                    : '#22c55e',
                                        }}>
                                            {isCancelled ? 'Cancelled' : isPast ? 'Completed' : 'Confirmed'}
                                        </span>

                                        {!isCancelled && !isPast && (
                                            <button
                                                onClick={() => handleCancel(booking.id)}
                                                style={{
                                                    padding: '6px 14px',
                                                    borderRadius: 6,
                                                    border: '1px solid rgba(239,68,68,0.3)',
                                                    background: 'rgba(239,68,68,0.1)',
                                                    color: '#ef4444',
                                                    fontSize: 12,
                                                    fontWeight: 500,
                                                    cursor: 'pointer',
                                                }}
                                            >
                                                Cancel
                                            </button>
                                        )}
                                    </div>
                                </div>

                                <div style={{
                                    marginTop: 12,
                                    paddingTop: 12,
                                    borderTop: '1px solid rgba(255,255,255,0.05)',
                                    fontSize: 12,
                                    color: 'rgba(255,255,255,0.3)',
                                }}>
                                    Code: <span style={{ color: '#a78bfa', fontFamily: 'monospace' }}>
                                        {booking.bookingCode}
                                    </span>
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>

            <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
        </div>
    );
}
