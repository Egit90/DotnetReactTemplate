import { ResetPasswordForm } from "../components/ResetPasswordForm.tsx";
import { KeyRound } from "lucide-react";

export const ResetPassword = () => {
    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <div className="w-full max-w-md">
                {/* Header Card */}
                <div className="mb-6 rounded-t-xl bg-gradient-to-r from-indigo-500 to-indigo-600 p-8 text-center shadow-lg">
                    <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-white/20 backdrop-blur-sm">
                        <KeyRound className="h-8 w-8 text-white" />
                    </div>
                    <h1 className="text-2xl font-bold text-white">Reset Your Password</h1>
                    <p className="mt-2 text-sm text-indigo-100">
                        Enter your email and new password below
                    </p>
                </div>

                {/* Form Card */}
                <div className="rounded-b-xl bg-card p-8 shadow-xl border">
                    <ResetPasswordForm />
                </div>
            </div>
        </div>
    );
};