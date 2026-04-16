import { NavLink } from 'react-router-dom';

const navItems = [
  { icon: 'list_alt', label: 'Library', to: '/library' },
  { icon: 'history_edu', label: 'Submissions', to: '/submissions' },
  { icon: 'person', label: 'Profile', to: '/profile' },
  { icon: 'settings', label: 'Settings', to: '/settings' },
];

export default function SideNav() {
  return (
    <aside className="hidden md:flex flex-col w-64 fixed left-0 top-0 h-screen bg-surface-container z-40">
      <div className="p-6">
        <div className="flex items-center gap-3 mb-1">
          <div className="w-8 h-8 bg-surface-container-highest rounded-lg flex items-center justify-center border border-primary/20">
            <span className="material-symbols-outlined icon-fill text-primary text-lg">data_object</span>
          </div>
          <div>
            <h2 className="text-base font-bold font-headline text-primary">Project Explorer</h2>
            <p className="text-[0.6rem] text-slate-500 uppercase tracking-widest font-medium">
              Main Workspace
            </p>
          </div>
        </div>
      </div>

      <nav className="flex-1 flex flex-col mt-2 font-label text-xs font-medium tracking-wide">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `relative flex items-center gap-3 px-6 py-3 cursor-pointer transition-all duration-200 ${
                isActive
                  ? 'text-primary before:content-[""] before:absolute before:left-0 before:w-0.5 before:h-6 before:bg-primary before:rounded-full bg-primary/5'
                  : 'text-slate-400 hover:text-slate-200 hover:bg-white/5'
              }`
            }
          >
            <span className="material-symbols-outlined">{item.icon}</span>
            <span className="uppercase tracking-widest font-semibold">{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="p-6 mt-auto">
        <div className="bg-surface-container-lowest p-4 rounded-xl border border-outline-variant/10">
          <p className="text-[0.65rem] text-slate-500 mb-2 font-bold uppercase tracking-widest">
            Storage
          </p>
          <div className="w-full bg-surface-container-high h-1.5 rounded-full overflow-hidden">
            <div className="bg-primary w-3/4 h-full" />
          </div>
          <p className="text-[0.65rem] text-slate-400 mt-2">7.2 GB of 10 GB used</p>
        </div>
      </div>
    </aside>
  );
}
