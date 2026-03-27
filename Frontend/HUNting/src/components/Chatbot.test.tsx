import { render, screen, fireEvent } from '@testing-library/react';
import { expect, test, describe, vi } from 'vitest';
import { Chatbot } from './Chatbot'; 

globalThis.fetch = vi.fn() as any;

describe('Chatbot Component', () => {
  test('opens chatbot and renders input field', () => {
    render(<Chatbot />);

    const openButton = screen.getByRole('button');
    fireEvent.click(openButton);

    const inputElement = screen.getByRole('textbox');
    expect(inputElement).toBeInTheDocument();
  });

  test('allows user to type', () => {
    render(<Chatbot />);
    
    const openButton = screen.getByRole('button');
    fireEvent.click(openButton);

    const inputElement = screen.getByRole('textbox') as HTMLInputElement;

    fireEvent.change(inputElement, { target: { value: 'Hello World' } });
    expect(inputElement.value).toBe('Hello World');
  });
});
