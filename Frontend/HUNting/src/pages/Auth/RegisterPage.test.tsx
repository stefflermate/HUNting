import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, test, vi, beforeEach } from 'vitest';
import { RegisterPage } from './RegisterPage';
import { BrowserRouter } from 'react-router-dom';
import * as AuthContext from '@/context/AuthContext';

vi.mock('@/context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

describe('RegisterPage Component', () => {
  const mockRegister = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
    (AuthContext.useAuth as any).mockReturnValue({
      register: mockRegister,
      isLoading: false,
      isAuthenticated: false,
    });
  });

  const renderComponent = () => render(
    <BrowserRouter>
      <RegisterPage />
    </BrowserRouter>
  );

  test('calls register with correct data for Person', async () => {
    const { container } = renderComponent();
    
    // Find inputs
    const inputs = container.querySelectorAll('input');
    const nameInput = Array.from(inputs).find(i => i.id === 'name')!;
    const emailInput = Array.from(inputs).find(i => i.id === 'email')!;
    const passwordInput = Array.from(inputs).find(i => i.id === 'password')!;
    const phoneInput = Array.from(inputs).find(i => i.id === 'phone')!;

    fireEvent.change(nameInput, { target: { value: 'John Doe' } });
    fireEvent.change(emailInput, { target: { value: 'john@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.change(phoneInput, { target: { value: '+36301234567' } });

    // Submit
    const submitButton = container.querySelector('button[type="submit"]')!;
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith(expect.objectContaining({
        name: 'John Doe',
        email: 'john@example.com',
        password: 'password123',
        phone: '+36301234567',
        accountType: 'Person'
      }));
    });
  });

  test('displays error message when registration fails', async () => {
    mockRegister.mockRejectedValue(new Error('Email already exists'));
    const { container } = renderComponent();

    const inputs = container.querySelectorAll('input');
    const nameInput = Array.from(inputs).find(i => i.id === 'name')!;
    const emailInput = Array.from(inputs).find(i => i.id === 'email')!;
    const passwordInput = Array.from(inputs).find(i => i.id === 'password')!;

    fireEvent.change(nameInput, { target: { value: 'John' } });
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'pass' } });
    
    fireEvent.click(container.querySelector('button[type="submit"]')!);

    await waitFor(() => {
      expect(screen.getByText('Email already exists')).toBeInTheDocument();
    });
  });

  test('redirects if already authenticated', () => {
    (AuthContext.useAuth as any).mockReturnValue({
      register: mockRegister,
      isLoading: false,
      isAuthenticated: true,
    });
    
    const { container } = renderComponent();
    expect(container.querySelector('#email')).not.toBeInTheDocument();
  });
});
