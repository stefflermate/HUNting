import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';
import { NotFoundPage } from './NotFoundPage';
import { BrowserRouter } from 'react-router-dom';

describe('NotFoundPage Component', () => {
  test('renders 404 text and return link', () => {
    render(
      <BrowserRouter>
        <NotFoundPage />
      </BrowserRouter>
    );

    expect(screen.getByText('404')).toBeInTheDocument();
    
    // check link by href
    const homeLink = screen.getByRole('link');
    expect(homeLink).toHaveAttribute('href', '/');
  });
});
