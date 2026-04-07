import { Link, useLocation } from 'react-router-dom';

const navLinks = [
  { label: 'Library', to: '/library' },
  { label: 'Editor', to: '/editor' },
  { label: 'Submissions', to: '/submissions' },
  { label: 'Profile', to: '/profile' },
];

export default function TopBar() {
  const { pathname } = useLocation();

  return (
    <header className="sticky top-0 z-50 flex items-center justify-between px-6 py-3 w-full bg-[#0b1326]/80 backdrop-blur-xl border-b border-[#45474b]/15">
      <div className="flex items-center gap-6">
        <Link to="/" className="flex items-center gap-2">
          <span className="material-symbols-outlined text-primary">terminal</span>
          <span className="font-headline text-xl font-bold tracking-tighter text-primary">
            Synthetica Code
          </span>
        </Link>
        <nav className="hidden md:flex items-center gap-6 text-sm font-medium">
          {navLinks.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className={`font-headline tracking-tight transition-colors ${
                pathname.startsWith(link.to)
                  ? 'text-primary border-b-2 border-primary pb-0.5'
                  : 'text-slate-400 hover:text-slate-100'
              }`}
            >
              {link.label}
            </Link>
          ))}
        </nav>
      </div>
      <div className="flex items-center gap-3">
        <button className="p-2 text-slate-400 hover:bg-surface-bright/20 rounded-lg transition-colors">
          <span className="material-symbols-outlined">notifications</span>
        </button>
        <button className="p-2 text-slate-400 hover:bg-surface-bright/20 rounded-lg transition-colors">
          <span className="material-symbols-outlined">settings</span>
        </button>
        <div className="w-8 h-8 rounded-full bg-surface-container-highest border border-primary/20 overflow-hidden flex items-center justify-center">
          <span className="material-symbols-outlined text-primary text-sm">person</span>
        </div>
      </div>
    </header>
  );
}
