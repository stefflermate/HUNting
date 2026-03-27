import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';
import { AppFooter } from './AppFooter';

describe('AppFooter Component', () => {
  test('renders footer text correctly', () => {
    render(<AppFooter />);

    expect(screen.getByText(/HUNting Portal/i)).toBeInTheDocument();
    
    const currentYear = new Date().getFullYear();
    const copyrightText = new RegExp(currentYear.toString());
    expect(screen.getByText(copyrightText)).toBeInTheDocument();
  });
});
