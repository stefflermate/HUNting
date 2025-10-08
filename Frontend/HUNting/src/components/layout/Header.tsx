import { useState } from 'react';
import { Button } from '@/components/ui/button';

export default function Header() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <header className="bg-gradient-to-r from-yellow-900 to-yellow-800 text-white shadow-md fixed top-0 left-0 w-full z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <a href="/" className="text-lg sm:text-xl font-bold tracking-wide text-white flex items-center">
            🦌 <span className="ml-1">VadászVilág</span>
          </a>

          {/* Desktop Nav */}
          <nav className="hidden md:flex space-x-6 items-center">
            <NavLink label="Főoldal" href="#" />
            <NavLink label="Rólunk" href="#" />
            <NavLink label="Területek" href="#" />
            <NavLink label="Kapcsolat" href="#" />

            <Button className="ml-4 bg-amber-200 text-yellow-900 font-semibold hover:bg-amber-300 transition">
              Bejelentkezés
            </Button>
          </nav>

          {/* Mobile menu button */}
          <div className="md:hidden">
            <button
              onClick={() => setIsOpen(!isOpen)}
              className="text-white focus:outline-none"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                {isOpen ? (
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                ) : (
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M4 6h16M4 12h16M4 18h16"
                  />
                )}
              </svg>
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Menu */}
      {isOpen && (
        <div className="md:hidden bg-yellow-900 px-4 pt-2 pb-4 shadow text-white space-y-2">
          <MobileNavItem label="Főoldal" href="#" />
          <MobileNavItem label="Rólunk" href="#" />
          <MobileNavItem label="Területek" href="#" />
          <MobileNavItem label="Kapcsolat" href="#" />
          <Button className="w-full bg-amber-200 text-yellow-900 hover:bg-amber-300 transition">
            Bejelentkezés
          </Button>
        </div>
      )}
    </header>
  );
}

function NavLink({ label, href }: { label: string; href: string }) {
  return (
    <a
      href={href}
      className="text-white hover:text-amber-300 transition text-sm sm:text-base"
    >
      {label}
    </a>
  );
}

function MobileNavItem({ label, href }: { label: string; href: string }) {
  return (
    <a
      href={href}
      className="block text-white hover:text-amber-300 text-base transition"
    >
      {label}
    </a>
  );
}
