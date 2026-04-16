import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import MonacoEditor from '@monaco-editor/react';

type Tab = 'Description' | 'Editor' | 'Submissions';

const defaultCode = `class Solution:
    def merge(self, intervals: List[List[int]]) -> List[List[int]]:
        # Sort intervals by start time
        intervals.sort(key=lambda x: x[0])

        merged = []
        for interval in intervals:
            # If list is empty or no overlap
            if not merged or merged[-1][1] < interval[0]:
                merged.append(interval)
            else:
                # There is overlap, merge current
                merged[-1][1] = max(merged[-1][1], interval[1])

        return merged
`;

const testCases = [
  { label: 'Case 1', input: 'intervals = [[1,3],[2,6],[8,10],[15,18]]', output: '[[1,6],[8,10],[15,18]]' },
  { label: 'Case 2', input: 'intervals = [[1,4],[4,5]]', output: '[[1,5]]' },
  { label: 'Case 3', input: 'intervals = [[1,4],[2,3]]', output: '[[1,4]]' },
];

const submissions = [
  { id: 1, status: 'Accepted',        lang: 'Python 3', runtime: '42ms',  memory: '14.8 MB', time: '2h ago' },
  { id: 2, status: 'Wrong Answer',    lang: 'Python 3', runtime: '—',     memory: '—',       time: '3h ago' },
  { id: 3, status: 'Time Limit',      lang: 'Python 3', runtime: '—',     memory: '—',       time: '1d ago' },
  { id: 4, status: 'Accepted',        lang: 'Python 3', runtime: '51ms',  memory: '15.1 MB', time: '2d ago' },
];

export default function CodeEditor() {
  const [searchParams] = useSearchParams();
  const initialTab = (searchParams.get('tab') as Tab) ?? 'Description';
  const [activeTab, setActiveTab] = useState<Tab>(initialTab);
  const [activeCase, setActiveCase] = useState(0);
  const navigate = useNavigate();

  return (
    <div className="flex flex-col min-h-screen">
      {/* Problem Context Header */}
      <section className="px-6 py-4 bg-surface-container-low">
        <div className="flex flex-col gap-1">
          <div className="flex items-center gap-2">
            <span className="text-xs font-label uppercase tracking-widest text-tertiary font-bold">
              Problem 56
            </span>
            <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-tertiary-container text-tertiary border border-tertiary/20">
              Medium
            </span>
          </div>
          <h1 className="text-2xl font-headline font-bold tracking-tight text-on-surface">
            Merge Intervals
          </h1>
        </div>
      </section>

      {/* Tab Navigation */}
      <nav className="flex px-6 bg-surface-container-low border-b border-outline-variant/10">
        {(['Description', 'Editor', 'Submissions'] as Tab[]).map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`flex-1 py-4 text-sm font-semibold border-b-2 transition-colors ${
              activeTab === tab
                ? 'text-primary border-primary bg-primary/5'
                : 'text-slate-400 border-transparent hover:text-slate-200'
            }`}
          >
            {tab}
          </button>
        ))}
      </nav>

      {/* Description Tab */}
      {activeTab === 'Description' && (
        <section className="flex-1 px-6 py-6 overflow-y-auto thin-scrollbar">
          <p className="text-on-surface-variant text-sm leading-relaxed mb-4">
            Given an array of <code className="text-secondary font-mono bg-surface-container px-1 rounded">intervals</code> where{' '}
            <code className="text-secondary font-mono bg-surface-container px-1 rounded">intervals[i] = [start_i, end_i]</code>, merge all overlapping intervals,
            and return an array of the non-overlapping intervals that cover all the intervals in the input.
          </p>
          <div className="space-y-4 mb-6">
            {testCases.map((tc, i) => (
              <div key={i} className="bg-surface-container rounded-lg p-4 border border-outline-variant/10">
                <div className="text-xs font-bold uppercase tracking-widest text-on-surface-variant mb-2">Example {i + 1}</div>
                <div className="text-xs font-mono space-y-1">
                  <div><span className="text-outline">Input:</span> <span className="text-on-surface">{tc.input}</span></div>
                  <div><span className="text-outline">Output:</span> <span className="text-primary">{tc.output}</span></div>
                </div>
              </div>
            ))}
          </div>
          <div className="bg-surface-container rounded-lg p-4 border border-outline-variant/10">
            <div className="text-xs font-bold uppercase tracking-widest text-on-surface-variant mb-2">Constraints</div>
            <ul className="text-xs font-mono text-on-surface-variant space-y-1">
              <li>• 1 &lt;= intervals.length &lt;= 10⁴</li>
              <li>• intervals[i].length == 2</li>
              <li>• 0 &lt;= start_i &lt;= end_i &lt;= 10⁴</li>
            </ul>
          </div>
        </section>
      )}

      {/* Editor Tab */}
      {activeTab === 'Editor' && (
        <div className="flex flex-col flex-1 min-h-0">
          {/* Editor Toolbar */}
          <section className="flex items-center justify-between px-6 py-3 bg-surface-container border-b border-outline-variant/5">
            <div className="flex items-center gap-2 px-3 py-1.5 rounded bg-surface-container-lowest border border-outline-variant/20 cursor-pointer hover:bg-surface-bright transition-colors">
              <span className="material-symbols-outlined text-sm text-primary">code</span>
              <span className="text-xs font-mono font-medium">Python 3</span>
              <span className="material-symbols-outlined text-xs text-on-surface-variant">expand_more</span>
            </div>
            <div className="flex items-center gap-3">
              <button className="p-2 text-on-surface-variant hover:text-primary transition-colors">
                <span className="material-symbols-outlined text-sm">refresh</span>
              </button>
              <button className="p-2 text-on-surface-variant hover:text-primary transition-colors">
                <span className="material-symbols-outlined text-sm">settings</span>
              </button>
              <button className="p-2 text-on-surface-variant hover:text-primary transition-colors">
                <span className="material-symbols-outlined text-sm">fullscreen</span>
              </button>
            </div>
          </section>

          {/* Code Editor */}
          <section style={{ height: 'clamp(250px, 40vh, 400px)' }}>
            <MonacoEditor
              height="100%"
              language="python"
              theme="vs-dark"
              defaultValue={defaultCode}
              options={{
                fontSize: 14,
                fontFamily: "'Fira Code', 'JetBrains Mono', monospace",
                fontLigatures: true,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                lineNumbers: 'on',
                renderLineHighlight: 'none',
                overviewRulerBorder: false,
                hideCursorInOverviewRuler: true,
                padding: { top: 16, bottom: 16 },
              }}
            />
          </section>

          {/* Console */}
          <section className="p-6 bg-surface-container border-t border-outline-variant/10">
            <div className="flex items-center gap-2 mb-4">
              <span className="text-xs font-bold uppercase tracking-widest text-on-surface-variant">Console</span>
              <div className="h-px flex-1 bg-outline-variant/10" />
            </div>
            <div className="bg-surface-container-lowest rounded-lg p-4 border border-outline-variant/10">
              <div className="flex gap-4 text-xs font-mono mb-4">
                {testCases.map((tc, i) => (
                  <button
                    key={tc.label}
                    onClick={() => setActiveCase(i)}
                    className={`pb-1 ${i === activeCase ? 'text-primary border-b border-primary' : 'text-on-surface-variant'}`}
                  >
                    {tc.label}
                  </button>
                ))}
              </div>
              <div className="space-y-3">
                <div>
                  <div className="text-[10px] uppercase text-outline-variant font-bold mb-1">Input</div>
                  <div className="bg-surface-container p-2 rounded text-secondary-fixed-dim text-xs font-mono">
                    {testCases[activeCase].input}
                  </div>
                </div>
                <div>
                  <div className="text-[10px] uppercase text-outline-variant font-bold mb-1">Expected Output</div>
                  <div className="bg-surface-container p-2 rounded text-primary text-xs font-mono">
                    {testCases[activeCase].output}
                  </div>
                </div>
              </div>
            </div>
          </section>
        </div>
      )}

      {/* Submissions Tab */}
      {activeTab === 'Submissions' && (
        <section className="flex-1 px-6 py-6 overflow-y-auto thin-scrollbar">
          <div className="space-y-2">
            {submissions.map((s) => (
              <div
                key={s.id}
                onClick={() => navigate(`/submissions/${s.id}`)}
                className="bg-surface-container rounded-lg px-4 py-3 border border-outline-variant/10 flex items-center gap-4 cursor-pointer hover:bg-surface-container-high transition-colors"
              >
                <span className={`text-sm font-bold shrink-0 ${
                  s.status === 'Accepted' ? 'text-primary' : 'text-error'
                }`}>
                  {s.status}
                </span>
                <span className="hidden md:inline text-xs font-mono text-on-surface-variant w-20 shrink-0">{s.lang}</span>
                <span className="hidden md:inline text-xs font-mono text-on-surface-variant w-16 shrink-0">{s.runtime}</span>
                <span className="hidden md:inline text-xs font-mono text-on-surface-variant w-20 shrink-0">{s.memory}</span>
                <span className="text-xs text-outline ml-auto">{s.time}</span>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* Sticky Bottom Actions */}
      <footer className="sticky bottom-16 md:bottom-0 bg-surface-container-high/90 backdrop-blur-md border-t border-outline-variant/20 px-6 py-4 z-10">
        <div className="flex items-center gap-4 max-w-2xl mx-auto">
          <button className="flex-1 flex items-center justify-center gap-2 py-3 px-4 rounded-xl font-bold text-sm bg-surface-container-highest text-on-surface border border-outline-variant/30 hover:bg-surface-bright transition-colors">
            <span className="material-symbols-outlined text-lg">play_arrow</span>
            Run
          </button>
          <button
            onClick={() => navigate('/submissions/1')}
            className="flex-[2] flex items-center justify-center gap-2 py-3 px-4 rounded-xl font-bold text-sm bg-gradient-to-br from-primary to-on-primary-container text-on-primary shadow-lg shadow-primary/10 active:opacity-80 transition-all"
          >
            <span className="material-symbols-outlined icon-fill text-lg">publish</span>
            Submit
          </button>
        </div>
      </footer>
    </div>
  );
}
