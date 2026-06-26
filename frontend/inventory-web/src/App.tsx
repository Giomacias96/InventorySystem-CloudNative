import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';

import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Inventory from './pages/Inventory';
import Orders from './pages/Orders';
import Analytics from './pages/Analytics';

const router = createBrowserRouter([
  {
    // El Login va FUERA del layout porque no queremos que tenga la barra lateral
    path: '/login',
    element: <Login />,
  },
  {
    // Todas las páginas administrativas van anidadas AQUÍ
    path: '/',
    element: <DashboardLayout />,
    children: [
      {
        index: true, // Si entran a la raíz "/", los mandamos al dashboard
        element: <Navigate to="/dashboard" replace />,
      },
      {
        path: 'dashboard',
        element: <Dashboard />,
      },
      {
        path: 'inventory',
        element: <Inventory />,
      },
      {
        path: 'orders',
        element: <Orders />,
      },
      {
        path: 'analytics',
        element: <Analytics />,
      }
    ]
  },
  {
    path: '*',
    element: <h1>404 - Página no encontrada</h1>,
  }
]);

export default function App() {
  return (
    <RouterProvider router={router} />
  );
}