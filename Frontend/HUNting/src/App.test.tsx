import { render, screen } from '@testing-library/react';
import { expect, test } from 'vitest';
import App from './App';

test('renders App component without crashing', () => {
  render(<App />);
  const linkElement = screen.getByRole('main'); // Adjust according to your app
  expect(linkElement).toBeInTheDocument();
});
