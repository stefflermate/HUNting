import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ProfilePage } from './ProfilePage';
import { useAuth } from '@/context/AuthContext';
import { MemoryRouter } from 'react-router-dom';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

describe('ProfilePage', () => {
  it('redirects to login if user is not authenticated', () => {
    (useAuth as any).mockReturnValue({ user: null });
    
    render(
      <MemoryRouter>
        <ProfilePage />
      </MemoryRouter>
    );

    expect(screen.queryByText(/Udvozlunk ujra/i)).not.toBeInTheDocument();
  });

  it('renders Person profile correctly', () => {
    (useAuth as any).mockReturnValue({
      user: { name: 'Vadasz Pista', accountType: 'Person', email: 'pista@vadasz.hu', roles: ['Member'] }
    });

    render(
      <MemoryRouter>
        <ProfilePage />
      </MemoryRouter>
    );

    expect(screen.getByText(/Udvozlunk ujra, Vadasz Pista!/i)).toBeInTheDocument();
    expect(screen.getByText('Szemely')).toBeInTheDocument();
    expect(screen.getByText('pista@vadasz.hu')).toBeInTheDocument();
  });

  it('renders Society profile correctly', () => {
    (useAuth as any).mockReturnValue({
      user: { name: 'Buda Tarsasag', accountType: 'Society', email: 'buda@tarsasag.hu', roles: ['Admin'] }
    });

    render(
      <MemoryRouter>
        <ProfilePage />
      </MemoryRouter>
    );

    expect(screen.getByText(/Udvozlunk ujra, Buda Tarsasag!/i)).toBeInTheDocument();
    expect(screen.getByText('Tarsasag')).toBeInTheDocument();
    expect(screen.getByText('buda@tarsasag.hu')).toBeInTheDocument();
  });

  it('calls logout when Kijelentkezes button is clicked', () => {
    const logoutMock = vi.fn();
    (useAuth as any).mockReturnValue({
      user: { name: 'Admin', accountType: 'Person', email: 'admin@admin.hu', roles: [] },
      logout: logoutMock
    });

    render(
      <MemoryRouter>
        <ProfilePage />
      </MemoryRouter>
    );

    const logoutBtn = screen.getByRole('button', { name: /Kijelentkezes/i });
    fireEvent.click(logoutBtn);
    expect(logoutMock).toHaveBeenCalled();
  });
});
