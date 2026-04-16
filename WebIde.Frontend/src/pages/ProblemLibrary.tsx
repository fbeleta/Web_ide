import { useNavigate } from 'react-router-dom';

type Difficulty = 'Easy' | 'Medium' | 'Hard';

interface Problem {
  id: number;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  acceptance: number;
  solved: boolean;
}

const problems: Problem[] = [
  { id: 1,   title: 'Two Sum',                                        difficulty: 'Easy',   tags: ['Arrays', 'Hash Table'],          acceptance: 49.2, solved: true  },
  { id: 3,   title: 'Longest Substring Without Repeating Characters', difficulty: 'Medium', tags: ['Hash Table', 'Sliding Window'],   acceptance: 33.9, solved: false },
  { id: 4,   title: 'Median of Two Sorted Arrays',                    difficulty: 'Hard',   tags: ['Binary Search', 'Divide & Conq'], acceptance: 35.7, solved: false },
  { id: 20,  title: 'Valid Parentheses',                              difficulty: 'Easy',   tags: ['String', 'Stack'],                acceptance: 40.3, solved: true  },
  { id: 56,  title: 'Merge Intervals',                                difficulty: 'Medium', tags: ['Array', 'Sorting'],               acceptance: 46.1, solved: false },
  { id: 146, title: 'LRU Cache',                                      difficulty: 'Medium', tags: ['Linked List', 'Design'],          acceptance: 41.8, solved: false },
];

const difficultyStyles: Record<Difficulty, string> = {
  Easy:   'bg-primary/10  text-primary  border border-primary/20',
  Medium: 'bg-tertiary/10 text-tertiary border border-tertiary/20',
  Hard:   'bg-error/10    text-error    border border-error/20',
};

export default function ProblemLibrary() {
  const navigate = useNavigate();

  return (
    <div className="p-6 max-w-6xl mx-auto">
      {/* Daily Challenge Hero */}
      <section className="mb-10">
        <div className="relative overflow-hidden rounded-xl bg-gradient-to-br from-surface-container to-surface-container-lowest border border-primary/5 p-1">
          <div className="relative p-6 md:p-10 flex flex-col md:flex-row justify-between items-center gap-8">
            <div className="flex-1 space-y-4">
              <div className="inline-flex items-center gap-2 bg-tertiary-container text-tertiary px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest">
                <span className="material-symbols-outlined icon-fill text-sm">bolt</span>
                Daily Challenge
              </div>
              <h2 className="text-3xl md:text-4xl font-headline font-bold tracking-tight text-on-surface">
                Valid Sudoku Solver
              </h2>
              <p className="text-on-surface-variant max-w-xl font-body">
                Determine if a 9x9 Sudoku board is valid. Only the filled cells need to be validated.
              </p>
              <div className="flex flex-wrap gap-4 pt-2">
                <div className="flex items-center gap-2 text-sm text-secondary font-medium">
                  <span className="material-symbols-outlined">timer</span>
                  14:22:05 Left
                </div>
                <div className="flex items-center gap-2 text-sm text-primary font-medium">
                  <span className="material-symbols-outlined">trending_up</span>
                  +100 XP
                </div>
              </div>
              <div className="pt-4">
                <button
                  onClick={() => navigate('/editor?tab=Description')}
                  className="bg-gradient-to-r from-primary to-on-primary-container text-on-primary font-bold py-3 px-8 rounded-xl flex items-center gap-3 hover:opacity-90 transition-opacity shadow-lg shadow-primary/10"
                >
                  Start Solving
                  <span className="material-symbols-outlined">arrow_forward</span>
                </button>
              </div>
            </div>
            <div className="hidden md:flex w-56 h-56 bg-surface-container-high rounded-2xl border border-outline-variant/20 items-center justify-center rotate-3">
              <span className="material-symbols-outlined text-8xl text-primary opacity-20">grid_4x4</span>
            </div>
          </div>
        </div>
      </section>

      {/* Search + Filter */}
      <section className="mb-8 flex flex-col md:flex-row gap-4">
        <div className="flex-1 relative">
          <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant/50">search</span>
          <input
            className="w-full bg-surface-container-lowest border-none rounded-xl py-4 pl-12 pr-4 text-on-surface placeholder:text-outline focus:ring-1 focus:ring-primary/50 transition-all outline-none"
            placeholder="Search problems, topics, tags..."
            type="text"
          />
        </div>
        <button className="flex items-center justify-center gap-2 bg-surface-container-high border border-outline-variant/20 text-on-surface px-6 py-4 rounded-xl hover:bg-surface-bright/20 transition-all font-semibold">
          <span className="material-symbols-outlined">filter_list</span>
          Filters
        </button>
      </section>

      {/* Problem List */}
      <section className="space-y-3">
        <div className="flex justify-between items-center px-2 mb-4">
          <h3 className="font-headline font-bold text-xl text-on-surface">Problem Library</h3>
          <span className="text-xs text-outline-variant font-mono uppercase tracking-widest">
            1,452 Problems available
          </span>
        </div>

        {problems.map((problem) => (
          <div
            key={problem.id}
            onClick={() => navigate(`/editor?tab=Description`)}
            className="bg-surface-container rounded-xl p-4 md:p-5 flex flex-col md:flex-row items-start md:items-center justify-between gap-4 border border-transparent hover:border-primary/20 hover:bg-surface-container-high transition-all group cursor-pointer"
          >
            <div className="flex items-center gap-4 flex-1">
              <div
                className={`w-10 h-10 flex-shrink-0 flex items-center justify-center rounded-lg ${
                  problem.solved ? 'bg-primary/10 text-primary' : 'bg-surface-container-lowest text-outline-variant'
                }`}
              >
                <span className={`material-symbols-outlined ${problem.solved ? 'icon-fill' : ''}`}>
                  {problem.solved ? 'check_circle' : 'circle'}
                </span>
              </div>
              <div>
                <div className="flex items-center gap-3 flex-wrap">
                  <h4 className="text-base font-bold text-on-surface group-hover:text-primary transition-colors">
                    {problem.id}. {problem.title}
                  </h4>
                  <span className={`text-[10px] px-2 py-0.5 rounded font-bold uppercase tracking-tighter ${difficultyStyles[problem.difficulty]}`}>
                    {problem.difficulty}
                  </span>
                </div>
                <div className="flex flex-wrap gap-2 mt-1.5">
                  {problem.tags.map((tag) => (
                    <span key={tag} className="text-[10px] text-outline font-medium px-2 py-0.5 rounded bg-surface-container-lowest border border-outline-variant/10">
                      {tag}
                    </span>
                  ))}
                </div>
              </div>
            </div>
            <div className="flex items-center gap-8 w-full md:w-auto justify-between md:justify-end">
              <div className="flex flex-col items-end">
                <span className="text-xs text-outline font-medium uppercase tracking-widest">Acceptance</span>
                <span className="text-sm font-mono text-secondary">{problem.acceptance}%</span>
              </div>
              <button className="w-10 h-10 rounded-full border border-outline-variant/20 flex items-center justify-center text-outline group-hover:text-primary group-hover:border-primary/50 transition-all">
                <span className="material-symbols-outlined">chevron_right</span>
              </button>
            </div>
          </div>
        ))}
      </section>

      {/* Load More */}
      <div className="py-12 flex justify-center">
        <button className="flex items-center gap-2 text-outline hover:text-primary transition-colors font-medium">
          <span className="material-symbols-outlined">expand_more</span>
          Show More Problems
        </button>
      </div>
    </div>
  );
}
