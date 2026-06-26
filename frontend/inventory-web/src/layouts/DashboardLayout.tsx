import { Outlet, NavLink } from 'react-router-dom';
import { LayoutDashboard, Package, ShoppingCart, BarChart2, LogOut } from 'lucide-react';
import './DashboardLayout.css';

export default function DashboardLayout() {
  return (
    <div className="layout-container">
      {/* Barra Lateral (Sidebar) */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <h2>Inventory ERP</h2>
        </div>
        
        <nav className="sidebar-nav">
          {/* NavLink le agrega la clase "active" automáticamente a la ruta actual */}
          <NavLink to="/dashboard" className="nav-item">
            <LayoutDashboard size={20} />
            <span>Dashboard</span>
          </NavLink>
          
          <NavLink to="/inventory" className="nav-item">
            <Package size={20} />
            <span>Inventario</span>
          </NavLink>
          
          <NavLink to="/orders" className="nav-item">
            <ShoppingCart size={20} />
            <span>Pedidos</span>
          </NavLink>
          
          <NavLink to="/analytics" className="nav-item">
            <BarChart2 size={20} />
            <span>Analíticos</span>
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <NavLink to="/login" className="nav-item logout">
            <LogOut size={20} />
            <span>Cerrar Sesión</span>
          </NavLink>
        </div>
      </aside>

      {/* Contenido Principal */}
      <main className="main-content">
        {/* Aquí es donde se inyectan las páginas dinámicamente (Dashboard, Inventory, etc.) */}
        <Outlet /> 
      </main>
    </div>
  );
}