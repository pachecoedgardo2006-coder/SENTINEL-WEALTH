import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../layout/MainLayout';
import HnwDashboardView from '../views/HnwDashboardView';
import CitizenScannerView from '../views/CitizenScannerView';
import LoginView from '../views/LoginView';
import RequireAuth from '../components/RequireAuth';

export const routes = [
  {
    path: '/login',
    element: <LoginView />,
  },
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        path: '/',
        element: (
          <RequireAuth>
            <HnwDashboardView />
          </RequireAuth>
        ),
      },
      {
        path: '/scanner',
        element: <CitizenScannerView />,
      }
    ]
  }
];

export const router = createBrowserRouter(routes);
