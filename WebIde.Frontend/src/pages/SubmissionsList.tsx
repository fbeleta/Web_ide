import { useNavigate } from 'react-router-dom';

type Status = 'Accepted' | 'Wrong Answer' | 'Time Limit' | 'Runtime Error';

interface Submission {
  id: number;
  problem: string;
  problemId: number;
  status: Status;
  lang: string;
  runtime: string;
  memory: string;
  time: string;
}

const submissions: Submission[] = [
  { id: 1, problem: 'Merge Intervals',                          problemId: 56,  status: 'Accepted',     lang: 'Python 3', runtime: '42ms',  memory: '14.8 MB', time: '2h ago'  },
  { id: 2, problem: 'Merge Intervals',                          problemId: 56,  status: 'Wrong Answer', lang: 'Python 3', runtime: '—',     memory: '—',       time: '3h ago'  },
  { id: 3, problem: 'Two Sum',                                  problemId: 1,   status: 'Accepted',     lang: 'Python 3', runtime: '38ms',  memory: '13.2 MB', time: '1d ago'  },
  { id: 4, problem: 'LRU Cache',                                problemId: 146, status: 'Time Limit',   lang: 'Python 3', runtime: '—',     memory: '—',       time: '2d ago'  },
  { id: 5, problem: 'Valid Parentheses',                        problemId: 20,  status: 'Accepted',     lang: 'Python 3', runtime: '29ms',  memory: '11.9 MB', time: '3d ago'  },
  { id: 6, problem: 'Longest Substring Without Repeating Chars', problemId: 3,  status: 'Runtime Error',lang: 'Python 3', runtime: '—',     memory: '—',       time: '4d ago'  },
  { id: 7, problem: 'Median of Two Sorted Arrays',              problemId: 4,   status: 'Accepted',     lang: 'Python 3', runtime: '51ms',  memory: '15.1 MB', time: '5d ago'  },
];

const statusStyle: Record<Status, string> = {
  'Accepted':     'text-primary',
  'Wrong Answer': 'text-error',
  'Time Limit':   'text-tertiary',
  'Runtime Error':'text-error',
};

export default function SubmissionsList() {
  const navigate = useNavigate();

  return (
    <div className="p-6 md:p-8 max-w-5xl mx-auto">
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-8">Submissions</h1>

      <div className="bg-surface-container rounded-xl border border-outline-variant/10 overflow-hidden">
        {/* Table header — hidden on mobile, shown at md+ */}
        <div className="hidden md:grid grid-cols-[2fr_1fr_1fr_1fr_1fr_1fr] gap-4 px-5 py-3 border-b border-outline-variant/10 text-[10px] uppercase tracking-widest font-bold text-outline-variant">
          <span>Problem</span>
          <span>Status</span>
          <span>Language</span>
          <span>Runtime</span>
          <span>Memory</span>
          <span>Time</span>
        </div>

        {submissions.map((s) => (
          <div
            key={s.id}
            onClick={() => navigate(`/submissions/${s.id}`)}
            className="border-b border-outline-variant/5 last:border-0 hover:bg-surface-container-high transition-colors cursor-pointer"
          >
            {/* Mobile layout */}
            <div className="md:hidden flex items-center justify-between px-5 py-4 gap-3">
              <div className="min-w-0">
                <span className={`text-xs font-bold ${statusStyle[s.status]}`}>{s.status}</span>
                <p className="text-sm font-medium text-on-surface truncate mt-0.5">{s.problemId}. {s.problem}</p>
              </div>
              <span className="text-xs text-outline shrink-0">{s.time}</span>
            </div>
            {/* Desktop layout */}
            <div className="hidden md:grid grid-cols-[2fr_1fr_1fr_1fr_1fr_1fr] gap-4 px-5 py-4 items-center">
              <span className="text-sm font-medium text-on-surface hover:text-primary transition-colors truncate">
                {s.problemId}. {s.problem}
              </span>
              <span className={`text-sm font-bold ${statusStyle[s.status]}`}>{s.status}</span>
              <span className="text-xs font-mono text-on-surface-variant">{s.lang}</span>
              <span className="text-xs font-mono text-on-surface-variant">{s.runtime}</span>
              <span className="text-xs font-mono text-on-surface-variant">{s.memory}</span>
              <span className="text-xs text-outline">{s.time}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
