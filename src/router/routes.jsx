import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../layout/MainLayout';
import HnwDashboardView from '../views/HnwDashboardView';
import CitizenScannerView from '../views/CitizenScannerView';

export const routes = [
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        path: '/',
        element: <HnwDashboardView />,
      },
      {
        path: '/scanner',
        element: <CitizenScannerView />,
      }
    ]
  }
];

export const router = createBrowserRouter(routes);
