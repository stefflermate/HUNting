import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "@/layouts/AppLayout";
import { AuthLayout } from "@/layouts/AuthLayout";
import { HomePage } from "@/pages/HomePage";
import { AdsListPage } from "@/pages/Ads/AdsListPage";
import { AdDetailPage } from "@/pages/Ads/AdDetailPage";
import { AdCreatePage } from "@/pages/Ads/AdCreatePage";
import { AdEditPage } from "@/pages/Ads/AdEditPage";
import { SocietiesPage } from "@/pages/Societies/SocietiesPage";
import { SocietyDetailPage } from "@/pages/Societies/SocietyDetailPage";
import { MySocietiesPage } from "@/pages/Societies/MySocietiesPage";
import { SocietyManagementPage } from "@/pages/Societies/SocietyManagementPage";
import { HuntingLogsPage } from "@/pages/HuntingLogs/HuntingLogsPage";
import { MapPage } from "@/pages/Map/MapPage";
import { LoginPage } from "@/pages/Auth/LoginPage";
import { RegisterPage } from "@/pages/Auth/RegisterPage";
import { ProfilePage } from "@/pages/Profile/ProfilePage";
import { AdminDashboardPage } from "@/pages/Admin/AdminDashboardPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { WeatherPage } from "@/pages/Weather/WeatherPage";

export const appRouter = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    errorElement: <NotFoundPage />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "ads", element: <AdsListPage /> },
      { path: "ads/new", element: <AdCreatePage /> },
      { path: "ads/:adId", element: <AdDetailPage /> },
      { path: "ads/:adId/edit", element: <AdEditPage /> },
      { path: "societies", element: <SocietiesPage /> },
      { path: "societies/mine", element: <MySocietiesPage /> },
      { path: "societies/manage", element: <SocietyManagementPage /> },
      { path: "societies/:societyId", element: <SocietyDetailPage /> },
      { path: "map", element: <MapPage /> },
      { path: "logs", element: <HuntingLogsPage /> },
      { path: "weather", element: <WeatherPage /> },
      { path: "profile", element: <ProfilePage /> },
      { path: "admin", element: <AdminDashboardPage /> },
    ],
  },
  {
    path: "/auth",
    element: <AuthLayout />,
    children: [
      { index: true, element: <Navigate to="/auth/login" replace /> },
      { path: "login", element: <LoginPage /> },
      { path: "register", element: <RegisterPage /> },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
