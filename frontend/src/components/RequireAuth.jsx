import React from 'react';
import { Navigate } from 'react-router-dom';
import { getSession } from '../api/session';

const RequireAuth = ({ children }) => {
  const session = getSession();
  if (!session?.id) {
    return <Navigate to="/login" replace />;
  }
  return children;
};

export default RequireAuth;
