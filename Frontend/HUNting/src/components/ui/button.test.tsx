import { render, screen, fireEvent } from '@testing-library/react';
import { expect, test, vi } from 'vitest';
import { Button } from './button'; // adjust according to the export inside button.tsx

test('renders button correctly', () => {
  render(<Button>Click me</Button>);
  const buttonElement = screen.getByRole('button', { name: /click me/i });
  expect(buttonElement).toBeInTheDocument();
});

test('handles click events', () => {
  const handleClick = vi.fn();
  render(<Button onClick={handleClick}>Click me</Button>);
  const buttonElement = screen.getByRole('button', { name: /click me/i });
  
  fireEvent.click(buttonElement);
  
  expect(handleClick).toHaveBeenCalledTimes(1);
});

test('can be disabled', () => {
  render(<Button disabled>Disabled Button</Button>);
  const buttonElement = screen.getByRole('button', { name: /disabled button/i });
  
  expect(buttonElement).toBeDisabled();
});
