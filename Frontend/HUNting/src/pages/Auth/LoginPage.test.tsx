import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, test, vi, beforeEach } from 'vitest';
import { LoginPage } from './LoginPage';
import { BrowserRouter } from 'react-router-dom';
import * as AuthContext from '@/context/AuthContext';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
  AuthProvider: ({ children }: any) => <div>{children}</div>
}));

describe('LoginPage Component', () => {
  const mockLogin = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
    (AuthContext.useAuth as any).mockReturnValue({
      login: mockLogin,
      isLoading: false,
      isAuthenticated: false,
    });
  });

  const renderComponent = () => render(
    <BrowserRouter>
      <LoginPage />
    </BrowserRouter>
  );

  test('renders login form', () => {
    const { container } = renderComponent();
    expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument();
    expect(container.querySelector('#email')).toBeInTheDocument();
    expect(container.querySelector('#password')).toBeInTheDocument();
    // Use .not.toBeNull() or generic match for button
    const button = container.querySelector('button[type="submit"]');
    expect(button).toBeInTheDocument();
  });

  test('calls login with correct credentials on submit', async () => {
    const { container } = renderComponent();
    const emailInput = container.querySelector('#email') as HTMLInputElement;
    const passwordInput = container.querySelector('#password') as HTMLInputElement;
    const submitButton = container.querySelector('button[type="submit"]') as HTMLButtonElement;

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({ email: 'test@example.com', password: 'password123' });
    });
  });

  test('displays error message when login fails', async () => {
    mockLogin.mockRejectedValue(new Error('Hibás jelszó'));
    const { container } = renderComponent();

    const emailInput = container.querySelector('#email') as HTMLInputElement;
    const passwordInput = container.querySelector('#password') as HTMLInputElement;
    const submitButton = container.querySelector('button[type="submit"]') as HTMLButtonElement;

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'wrong' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Hibás jelszó')).toBeInTheDocument();
    });
  });

  test('redirects if already authenticated', () => {
    (AuthContext.useAuth as any).mockReturnValue({
      login: mockLogin,
      isLoading: false,
      isAuthenticated: true,
    });
    
    const { container } = renderComponent();
    expect(container.querySelector('#email')).not.toBeInTheDocument();
  });
});
