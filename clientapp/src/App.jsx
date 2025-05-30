import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

import LoginPage from './LoginPage';
import RegisterPage from './RegisterPage';
import AdminPanel from './AdminPanel';
import QuestionEditor from './QuestionEditor';
import UserQuizPage from './UserQuizPage'; // ← вот эта строка обязательна

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<UserQuizPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/admin/questions" element={<AdminPanel />} />
        <Route path="/admin/questions/new" element={<QuestionEditor />} />
        <Route path="/admin/questions/edit/:id" element={<QuestionEditor />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
