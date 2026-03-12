import { render, screen, fireEvent } from '@testing-library/react';
import { expect, test, describe, vi } from 'vitest';
import Chatbot from './Chatbot'; // Adjust based on your export (default or named)

// Mock fetch or services if Chatbot relies on window.fetch or custom hooks
global.fetch = vi.fn();

describe('Chatbot Component', () => {
  test('renders chatbot input field', () => {
    // If your Chatbot needs specific context, wrap it inside providers here.
    render(<Chatbot />);
    
    // Look for placeholder or role, depending on how Chatbot is built
    const inputElement = screen.getByRole('textbox'); 
    expect(inputElement).toBeInTheDocument();
  });

  test('allows user to type', () => {
    render(<Chatbot />);
    const inputElement = screen.getByRole('textbox') as HTMLInputElement;
    
    fireEvent.change(inputElement, { target: { value: 'Hello World' } });
    expect(inputElement.value).toBe('Hello World');
  });
});
