import { useRoutes } from 'react-router-dom';
import { ProtectedRoute } from "./ProtectedRoute.tsx";
import { SignIn } from '@/features/users/auth/routes/SignIn.tsx';
import { SignOut } from '@/features/users/auth/routes/SignOut.tsx';
import { SignUp } from '@/features/users/auth/routes/SignUp.tsx';
import { SignUpExternal } from '@/features/users/auth/routes/SignUpExternal.tsx';
import { MyAccount } from '@/features/users/account/routes/MyAccount.tsx';
import { Home } from '@/features/users/misc/routes/Home.tsx';
import { SignUpConfirmation } from '@/features/users/auth/routes/SignUpConfirmation.tsx';
import { ResendEmailConfirmation } from '@/features/users/account/routes/ResendEmailConfirmation.tsx';
import { ChallengeCallback } from '@/features/users/auth/routes/ChallengeCallback.tsx';
import { ForgotPassword } from '@/features/users/account/routes/ForgotPassword.tsx';
import { ResetPassword } from '@/features/users/account/routes/ResetPassword.tsx';
import { ResetPasswordConfirmation } from '@/features/users/account/routes/ResetPasswordConfirmation.tsx';
import { ConfirmEmail } from '@/features/users/account/routes/ConfirmEmail.tsx';
import { Unauthorized } from '@/features/users/misc/routes/Unauthorized.tsx';
import { NotFound } from '@/features/users/misc/routes/NotFound.tsx';
import { RoleProtectedRoute } from './ProtectedRoutes.tsx';
import MainLayout from '@/features/users/MainLayout.tsx';
import AdminHome from '@/features/admin/home/route/home.tsx';
import { AdminLayout } from '@/features/admin/layout/admin-layout.tsx';
import { UserManagement } from '@/features/admin/userManagement/route/page.tsx';
import { Logs } from '@/features/admin/logs/route/page.tsx';
import { SettingsPage } from '@/features/admin/settings/route/page.tsx';

export const AppRoutes = () => {
    const routes = [
        { path: '/signin', element: <SignIn /> },
        { path: '/signout', element: <SignOut /> },

        { path: '/signup', element: <SignUp /> },
        // Uncomment the following line to enable sign up with additional fields
        // {path: '/signup', element: <SignUpExtended/>},

        { path: '/signup-external', element: <SignUpExternal /> },
        // Uncomment the following line to enable external sign up with additional fields
        // {path: '/signup-external', element: <SignUpExternalExtended/>},
        {
            element: <MainLayout />,
            children: [
                { path: '/profile', element: <ProtectedRoute><MyAccount /></ProtectedRoute> },
                { path: '/', element: <Home /> },
                { path: '/signup/confirmation', element: <SignUpConfirmation /> },
                { path: '/resend-confirm-email', element: <ResendEmailConfirmation /> },
                { path: '/external-challenge-callback/:provider', element: <ChallengeCallback /> },
                { path: '/forgot-password', element: <ForgotPassword /> },
                { path: '/reset-password', element: <ResetPassword /> },
                { path: '/reset-password/confirmation', element: <ResetPasswordConfirmation /> },
                { path: '/confirm-email', element: <ConfirmEmail /> },
                { path: '/unauthorized', element: <Unauthorized /> },
                { path: '*', element: <NotFound /> },
            ]
        },
        {
            path: "/admin",
            element: <RoleProtectedRoute allowedRoles={["Admin"]}><AdminLayout /> </RoleProtectedRoute>,
            children: [
                { index: true, element: <AdminHome /> },
                { path: 'user-management', element: <UserManagement /> },
                { path: 'logs', element: <Logs /> },
                { path: 'settings', element: <SettingsPage /> },
                { path: '*', element: <NotFound /> }
            ]
        }
    ];

    const element = useRoutes([...routes]);

    return <>{element}</>;
};
