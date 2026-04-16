import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from './components/layout/AppLayout';
import ProblemLibrary from './pages/ProblemLibrary';
import CodeEditor from './pages/CodeEditor';
import SubmissionResults from './pages/SubmissionResults';
import UserProfile from './pages/UserProfile';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route index element={<Navigate to="/library" replace />} />
          <Route path="/library" element={<ProblemLibrary />} />
          <Route path="/editor" element={<CodeEditor />} />
          <Route path="/submissions" element={<SubmissionResults />} />
          <Route path="/profile" element={<UserProfile />} />
          <Route path="*" element={<Navigate to="/library" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
