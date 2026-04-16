const activityData = Array.from({ length: 84 }, () => {
  const intensities = [0, 0, 0.2, 0.4, 0.6, 0.8, 1];
  return intensities[Math.floor(Math.random() * intensities.length)];
});

const orgs = [
  {
    name: 'Advanced Algorithms 101',
    icon: 'functions',
    members: '12.4k',
    activeSets: 42,
    color: 'text-slate-400',
  },
  {
    name: 'Neural Arch Systems',
    icon: 'hub',
    members: '3.1k',
    activeSets: 18,
    color: 'text-slate-400',
  },
];

const teams = [
  {
    name: 'Team Zenith',
    sub: 'Core Infrastructure Taskforce',
    activeProblems: 8,
    live: true,
  },
  {
    name: 'Quantum Solvers',
    sub: '3 Members • 2 Problem Sets',
    activeProblems: 0,
    live: false,
  },
];

export default function UserProfile() {
  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto">
      {/* Profile Header */}
      <section className="flex flex-col md:flex-row items-center md:items-end gap-8 mb-12">
        <div className="relative">
          <div className="w-32 h-32 rounded-2xl border-2 border-primary/20 bg-surface-container-high ring-offset-4 ring-offset-surface ring-2 ring-primary/5 overflow-hidden flex items-center justify-center">
            <span className="material-symbols-outlined text-7xl text-on-surface-variant/30">person</span>
          </div>
          <div className="absolute -bottom-2 -right-2 w-8 h-8 bg-primary text-on-primary rounded-full flex items-center justify-center shadow-lg">
            <span className="material-symbols-outlined icon-fill text-sm">verified</span>
          </div>
        </div>
        <div className="flex-1 text-center md:text-left">
          <div className="flex items-center justify-center md:justify-start gap-3 mb-2">
            <h1 className="text-4xl font-headline font-bold tracking-tighter text-on-surface">Alex Chen</h1>
            <span className="px-2 py-0.5 rounded bg-primary/10 text-primary text-[0.6rem] font-bold uppercase tracking-widest border border-primary/20">
              Elite Tier
            </span>
          </div>
          <p className="text-slate-400 font-medium mb-1 flex items-center justify-center md:justify-start gap-2 text-sm">
            <span className="material-symbols-outlined text-sm">location_on</span>
            San Francisco, CA • Senior Systems Architect
          </p>
          <p className="text-slate-500 text-xs mb-4 flex items-center justify-center md:justify-start gap-2">
            <span className="material-symbols-outlined text-xs">alternate_email</span>
            alx_coder_99
            <span className="w-1 h-1 rounded-full bg-outline-variant/40 inline-block" />
            <span className="material-symbols-outlined text-xs">calendar_today</span>
            Joined Oct 2023
          </p>
          <div className="flex gap-3 justify-center md:justify-start">
            <button className="bg-gradient-to-r from-primary to-on-primary-container text-on-primary px-6 py-2.5 rounded-xl font-bold text-sm shadow-lg shadow-primary/10 hover:opacity-90 active:scale-95 transition-all">
              Follow
            </button>
            <button className="px-6 py-2.5 rounded-xl border border-secondary/20 text-secondary font-bold text-sm hover:bg-secondary/5 transition-all">
              Message
            </button>
          </div>
        </div>
      </section>

      {/* Stats Grid */}
      <section className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
        {[
          { icon: 'code',        label: 'Solved Problems', value: '1,284', sub: '+12 this week',   color: 'text-primary',   bg: 'bg-primary/10',   sub_color: 'text-primary'   },
          { icon: 'leaderboard', label: 'Global Ranking',  value: '#412',  sub: 'Top 0.8%',        color: 'text-secondary', bg: 'bg-secondary/10', sub_color: 'text-secondary' },
          { icon: 'verified',    label: 'Success Rate',    value: '94.2%', sub: 'Consistent',      color: 'text-tertiary',  bg: 'bg-tertiary/10',  sub_color: 'text-tertiary'  },
        ].map((stat) => (
          <div
            key={stat.label}
            className="bg-surface-container-low p-6 rounded-2xl border border-outline-variant/5 hover:border-primary/20 transition-all group"
          >
            <div className="flex justify-between items-start mb-4">
              <div className={`p-2 ${stat.bg} rounded-lg ${stat.color}`}>
                <span className="material-symbols-outlined">{stat.icon}</span>
              </div>
              <span className={`text-[0.65rem] font-bold ${stat.sub_color}`}>{stat.sub}</span>
            </div>
            <div className="text-3xl font-bold text-on-surface mb-1">{stat.value}</div>
            <div className="text-xs text-slate-500 font-medium uppercase tracking-wider">{stat.label}</div>
          </div>
        ))}
      </section>

      {/* Organizations & Teams */}
      <section className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
        {/* Organizations */}
        <div>
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-headline font-bold tracking-tight">Organizations</h2>
            <button className="text-primary text-xs font-bold uppercase tracking-widest hover:underline">
              View All
            </button>
          </div>
          <div className="space-y-4">
            {orgs.map((org) => (
              <div
                key={org.name}
                className="bg-surface-container p-5 rounded-2xl border border-outline-variant/5 flex items-center gap-5 hover:bg-surface-container-high transition-colors cursor-pointer"
              >
                <div className="w-14 h-14 rounded-xl bg-surface-container-lowest flex items-center justify-center border border-outline-variant/20">
                  <span className={`material-symbols-outlined text-2xl ${org.color}`}>{org.icon}</span>
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-on-surface mb-0.5">{org.name}</h3>
                  <div className="text-xs text-slate-500 flex items-center gap-3">
                    <span className="flex items-center gap-1">
                      <span className="material-symbols-outlined text-xs">groups</span>
                      {org.members}
                    </span>
                    <span className="flex items-center gap-1">
                      <span className="material-symbols-outlined text-xs">terminal</span>
                      {org.activeSets} Active Sets
                    </span>
                  </div>
                </div>
                <span className="material-symbols-outlined text-slate-500">chevron_right</span>
              </div>
            ))}
          </div>
        </div>

        {/* Teams */}
        <div>
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-headline font-bold tracking-tight">Teams</h2>
            <button className="text-primary text-xs font-bold uppercase tracking-widest hover:underline">
              Manage Teams
            </button>
          </div>
          <div className="space-y-4">
            {teams.map((team) => (
              <div
                key={team.name}
                className="bg-surface-container-low p-5 rounded-2xl border border-outline-variant/5 relative overflow-hidden"
              >
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="font-bold text-lg text-on-surface">{team.name}</h3>
                    <p className="text-xs text-slate-500">{team.sub}</p>
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <div className="text-xs text-on-surface-variant flex items-center gap-2">
                    <span
                      className={`w-2 h-2 rounded-full ${team.live ? 'bg-primary shadow-sm shadow-primary animate-pulse' : 'bg-slate-600'}`}
                    />
                    {team.live ? `${team.activeProblems} Active Problems` : 'Inactive'}
                  </div>
                  {team.live && (
                    <span className="text-xs font-mono text-primary font-bold">LIVE SESSION</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Activity Heatmap */}
      <section className="bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/5">
        <h2 className="text-sm font-bold uppercase tracking-widest text-slate-500 mb-6 flex items-center gap-2">
          <span className="material-symbols-outlined text-sm">calendar_month</span>
          Activity Distribution
        </h2>
        <div className="flex flex-wrap gap-1.5">
          {activityData.map((intensity, i) => (
            <div
              key={i}
              className="w-3 h-3 rounded-sm"
              style={{
                backgroundColor:
                  intensity === 0
                    ? 'rgb(34 42 61)' // surface-container-high
                    : `rgba(78, 222, 163, ${intensity})`,
              }}
            />
          ))}
        </div>
      </section>
    </div>
  );
}
