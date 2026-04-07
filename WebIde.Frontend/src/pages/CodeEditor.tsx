import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

type Tab = 'Description' | 'Editor' | 'Submissions';

const codeLines = [
  { n: 1,  content: <><span className="code-keyword">class</span> <span className="code-func">Solution</span>:</> },
  { n: 2,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-keyword">def</span> <span className="code-func">merge</span>(self, intervals: List[List[int]]) -&gt; List[List[int]]:</> },
  { n: 3,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-comment"># Sort intervals by start time</span></> },
  { n: 4,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;intervals.sort(key=<span className="code-keyword">lambda</span> x: x[<span className="code-string">0</span>])</> },
  { n: 5,  content: <></> },
  { n: 6,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;merged = []</> },
  { n: 7,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-keyword">for</span> interval <span className="code-keyword">in</span> intervals:</> },
  { n: 8,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-comment"># If list is empty or no overlap</span></> },
  { n: 9,  content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-keyword">if not</span> merged <span className="code-keyword">or</span> merged[-<span className="code-string">1</span>][<span className="code-string">1</span>] &lt; interval[<span className="code-string">0</span>]:</> },
  { n: 10, content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;merged.append(interval)</> },
  { n: 11, content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-keyword">else</span>:</> },
  { n: 12, content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-comment"># There is overlap, merge current</span></> },
  { n: 13, content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;merged[-<span className="code-string">1</span>][<span className="code-string">1</span>] = max(merged[-<span className="code-string">1</span>][<span className="code-string">1</span>], interval[<span className="code-string">1</span>])</> },
  { n: 14, content: <></> },
  { n: 15, content: <>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span className="code-keyword">return</span> merged</> },
];

const testCases = ['Case 1', 'Case 2', 'Case 3'];

export default function CodeEditor() {
  const [activeTab, setActiveTab] = useState<Tab>('Editor');
  const [activeCase, setActiveCase] = useState(0);
  const navigate = useNavigate();

  return (
    <div className="flex flex-col min-h-[calc(100vh-3.5rem)]">
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
      <section className="flex-1 bg-surface-container-lowest font-mono text-sm leading-relaxed overflow-x-auto relative">
        <div className="flex h-full">
          {/* Line Numbers */}
          <div className="w-12 flex flex-col pt-4 items-center text-outline-variant/40 select-none border-r border-outline-variant/5 gap-0 leading-6">
            {codeLines.map((line) => (
              <span key={line.n} className="text-right w-full pr-3">{line.n}</span>
            ))}
          </div>
          {/* Code Buffer */}
          <div className="flex-1 pt-4 px-4 whitespace-pre leading-6 overflow-y-auto thin-scrollbar">
            {codeLines.map((line) => (
              <div key={line.n} className="min-h-6">{line.content}</div>
            ))}
          </div>
        </div>
        {/* Active line highlight */}
        <div className="absolute top-[218px] left-0 w-full h-6 bg-primary/5 border-y border-primary/10 pointer-events-none" />
      </section>

      {/* Console */}
      <section className="p-6 bg-surface-container border-t border-outline-variant/10">
        <div className="flex items-center gap-2 mb-4">
          <span className="text-xs font-bold uppercase tracking-widest text-on-surface-variant">Console</span>
          <div className="h-px flex-1 bg-outline-variant/10" />
        </div>
        <div className="bg-surface-container-lowest rounded-lg p-4 border border-outline-variant/10">
          <div className="flex gap-4 text-xs font-mono mb-3">
            {testCases.map((tc, i) => (
              <button
                key={tc}
                onClick={() => setActiveCase(i)}
                className={`pb-1 ${i === activeCase ? 'text-primary border-b border-primary' : 'text-on-surface-variant'}`}
              >
                {tc}
              </button>
            ))}
          </div>
          <div>
            <div className="text-[10px] uppercase text-outline-variant font-bold mb-1">Input</div>
            <div className="bg-surface-container p-2 rounded text-secondary-fixed-dim text-xs font-mono">
              intervals = [[1,3],[2,6],[8,10],[15,18]]
            </div>
          </div>
        </div>
      </section>

      {/* Sticky Bottom Actions */}
      <footer className="sticky bottom-0 bg-surface-container-high/90 backdrop-blur-md border-t border-outline-variant/20 px-6 py-4 z-10">
        <div className="flex items-center gap-4 max-w-2xl mx-auto">
          <button className="flex-1 flex items-center justify-center gap-2 py-3 px-4 rounded-xl font-bold text-sm bg-surface-container-highest text-on-surface border border-outline-variant/30 hover:bg-surface-bright transition-colors">
            <span className="material-symbols-outlined text-lg">play_arrow</span>
            Run
          </button>
          <button
            onClick={() => navigate('/submissions')}
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
