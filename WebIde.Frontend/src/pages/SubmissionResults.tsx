import { useNavigate } from 'react-router-dom';

const testCases = [
  { id: '01', label: 'Standard Graph Case',    passed: true  },
  { id: '02', label: 'Large Sparse Matrix',    passed: true  },
  { id: '03', label: 'Edge Connection Limit',  passed: true  },
  { id: '04', label: 'Disconnected Nodes',     passed: true  },
  { id: '05', label: 'Hidden Case: Cycle Detection', passed: null },
];

const codeLines = `class Solution:
    def twoSum(self, nums: List[int], target: int) -> List[int]:
        """
        :type nums: List[int]
        :type target: int
        :rtype: List[int]
        """
        prevMap = {}  # val : index

        for i, n in enumerate(nums):
            diff = target - n
            if diff in prevMap:
                return [prevMap[diff], i]
            prevMap[n] = i
        return`;

export default function SubmissionResults() {
  const navigate = useNavigate();

  return (
    <div className="p-6 md:p-8 pb-20 md:pb-8 max-w-6xl mx-auto space-y-8">
      {/* Status Header */}
      <header className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 bg-surface-container p-8 rounded-xl border border-primary/10 flex items-center gap-4">
          <span className="material-symbols-outlined icon-fill text-primary text-4xl">check_circle</span>
          <div>
            <h1 className="font-headline text-4xl font-bold tracking-tight text-primary">Accepted</h1>
            <p className="text-on-surface-variant font-label text-sm mt-1 opacity-80">
              Problem: Optimizing Graph Traversal with Dijkstra's Variant
            </p>
          </div>
        </div>
        <div className="bg-surface-container p-6 rounded-xl border border-outline-variant/20">
          <div className="space-y-4">
            <div className="flex justify-between items-center border-b border-outline-variant/10 pb-2">
              <span className="text-on-surface-variant text-sm">Runtime</span>
              <span className="text-primary font-mono font-medium">42 ms</span>
            </div>
            <div className="flex justify-between items-center border-b border-outline-variant/10 pb-2">
              <span className="text-on-surface-variant text-sm">Memory</span>
              <span className="text-secondary font-mono font-medium">14.8 MB</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-on-surface-variant text-sm">Efficiency</span>
              <span className="text-tertiary font-bold">Beats 87.4%</span>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Code Snippet */}
        <div className="lg:col-span-2 space-y-4">
          <div className="flex items-center justify-between px-2">
            <h2 className="font-headline text-xl font-bold tracking-tight flex items-center gap-2">
              <span className="material-symbols-outlined text-secondary">code</span>
              Submitted Solution
            </h2>
            <button className="text-xs font-mono text-on-surface-variant hover:text-primary transition-colors flex items-center gap-1">
              <span className="material-symbols-outlined text-sm">content_copy</span>
              Copy Code
            </button>
          </div>
          <div className="bg-surface-container-lowest rounded-xl overflow-hidden border border-outline-variant/20">
            {/* Traffic lights bar */}
            <div className="bg-surface-container-high px-4 py-2 flex items-center gap-2 border-b border-outline-variant/15">
              <div className="w-3 h-3 rounded-full bg-error/40" />
              <div className="w-3 h-3 rounded-full bg-tertiary/40" />
              <div className="w-3 h-3 rounded-full bg-primary/40" />
              <span className="ml-4 text-xs font-mono text-on-surface-variant opacity-60">solution.py</span>
            </div>
            <pre className="p-6 font-mono text-sm leading-relaxed text-on-surface overflow-x-auto thin-scrollbar">
              <code>{codeLines}</code>
            </pre>
          </div>
        </div>

        {/* Side Panels */}
        <div className="space-y-6">
          {/* Output Logs */}
          <div>
            <h2 className="font-headline text-lg font-bold tracking-tight flex items-center gap-2 mb-3">
              <span className="material-symbols-outlined text-tertiary">terminal</span>
              Output Logs
            </h2>
            <div className="bg-surface-container-lowest rounded-xl border border-outline-variant/10 overflow-hidden">
              <div className="p-4 space-y-4 font-mono text-xs">
                <div>
                  <p className="text-on-surface-variant/50 mb-1 uppercase text-[10px]">STDOUT</p>
                  <p className="text-on-surface">Calculated weights for 1024 nodes...</p>
                  <p className="text-on-surface">Memory allocation optimized.</p>
                </div>
                <div className="pt-2 border-t border-outline-variant/5">
                  <p className="text-on-surface-variant/50 mb-1 uppercase text-[10px]">STDERR</p>
                  <p className="text-error italic">None</p>
                </div>
                <div className="pt-2 border-t border-outline-variant/5 flex justify-between items-center">
                  <span className="text-on-surface-variant/50 text-[10px] uppercase">EXIT CODE</span>
                  <span className="bg-primary/20 text-primary px-2 py-0.5 rounded text-[10px] font-bold">0</span>
                </div>
              </div>
            </div>
          </div>

          {/* Test Cases */}
          <div>
            <h2 className="font-headline text-lg font-bold tracking-tight flex items-center gap-2 mb-3">
              <span className="material-symbols-outlined text-primary">fact_check</span>
              Test Cases
            </h2>
            <div className="space-y-2">
              {testCases.map((tc) => (
                <div
                  key={tc.id}
                  className={`p-4 rounded-lg flex items-center justify-between ${
                    tc.passed === null
                      ? 'bg-surface-container-high/30 border-l-4 border-outline-variant/40 cursor-not-allowed opacity-60'
                      : 'bg-surface-container-high/50 border-l-4 border-primary'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <span className="text-xs font-mono text-on-surface-variant">{tc.id}</span>
                    <span className="text-sm font-medium">{tc.label}</span>
                  </div>
                  {tc.passed === null ? (
                    <span className="material-symbols-outlined text-on-surface-variant/40 text-sm">lock</span>
                  ) : (
                    <span className="text-primary text-xs font-bold font-mono">PASSED</span>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Footer Actions */}
      <footer className="flex flex-col md:flex-row gap-4 pt-8 border-t border-outline-variant/10">
        <button
          onClick={() => navigate('/editor')}
          className="px-8 py-3 bg-gradient-to-br from-primary to-on-primary-container text-on-primary font-bold rounded-xl shadow-xl shadow-primary/10 hover:opacity-90 transition-all flex items-center justify-center gap-2"
        >
          <span className="material-symbols-outlined icon-fill">play_arrow</span>
          Next Problem
        </button>
        <button className="px-8 py-3 bg-transparent border border-outline-variant/40 text-secondary hover:bg-secondary/5 transition-all rounded-xl font-bold flex items-center justify-center gap-2">
          <span className="material-symbols-outlined">share</span>
          Share Solution
        </button>
        <div className="flex-1" />
        <button className="px-6 py-3 text-on-surface-variant hover:text-on-surface transition-colors flex items-center justify-center gap-2 font-medium">
          <span className="material-symbols-outlined">feedback</span>
          Report Issue
        </button>
      </footer>
    </div>
  );
}
