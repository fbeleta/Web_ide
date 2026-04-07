import { NavLink } from 'react-router-dom';

const navItems = [
  { icon: 'home', label: 'Home', to: '/library' },
  { icon: 'list_alt', label: 'Problems', to: '/library' },
  { icon: 'code', label: 'Editor', to: '/editor' },
  { icon: 'person', label: 'Profile', to: '/profile' },
];

export default function MobileNav() {
  return (
    <nav className="md:hidden fixed bottom-0 left-0 right-0 h-16 bg-surface-container/95 backdrop-blur-xl border-t border-outline-variant/10 px-6 flex justify-around items-center z-50">
      {navItems.map((item) => (
        <NavLink
          key={item.label}
          to={item.to}
          className={({ isActive }) =>
            `flex flex-col items-center gap-1 transition-colors ${
              isActive ? 'text-primary' : 'text-slate-400'
            }`
          }
        >
          <span className="material-symbols-outlined">{item.icon}</span>
          <span className="text-[0.6rem] uppercase tracking-widest font-bold">{item.label}</span>
        </NavLink>
      ))}
    </nav>
  );
}
