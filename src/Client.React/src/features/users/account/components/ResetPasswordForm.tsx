import { useNavigate, useSearchParams } from "react-router-dom";
import { SubmitHandler, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from '@/providers/AuthProvider.tsx';
import { extractApiErrors } from 'crystal-client/src/axios-utils.ts';
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

export const ResetPasswordForm = () => {
    const [params] = useSearchParams();
    const code = params.get("code");
    const navigate = useNavigate();
    if (!code) {
        throw new Error("Invalid code");
    }

    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const { authClient } = useAuth();

    const onSubmit: SubmitHandler<FormModel> = async (data) => {
        try {
            await authClient.account.resetPassword({ email: data.email, password: data.password, code: code });
            toast.success("Password reset successfully! Redirecting to login...");
            setTimeout(() => navigate("/reset-password/confirmation"), 1500);
        } catch (error) {
            const errors = extractApiErrors(error) ?? ["Error occurred"];
            errors.forEach(err => toast.error(err));
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} method="POST" noValidate={true}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="email">Email address</FieldLabel>
                    <Input
                        {...register("email")}
                        id="email"
                        type="email"
                        autoComplete="email"
                        placeholder="Enter your email"
                        required
                    />
                    {errors.email?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.email.message}
                        </p>
                    )}
                </Field>

                <Field>
                    <FieldLabel htmlFor="password">New Password</FieldLabel>
                    <Input
                        {...register("password")}
                        id="password"
                        type="password"
                        autoComplete="new-password"
                        placeholder="Enter new password"
                        required
                    />
                    {errors.password?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.password.message}
                        </p>
                    )}
                </Field>

                <Field>
                    <FieldLabel htmlFor="confirmPassword">Confirm Password</FieldLabel>
                    <Input
                        {...register("confirmPassword")}
                        id="confirmPassword"
                        type="password"
                        autoComplete="new-password"
                        placeholder="Confirm new password"
                        required
                    />
                    {errors.confirmPassword?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.confirmPassword.message}
                        </p>
                    )}
                </Field>

                <Field className="pt-2">
                    <Button type="submit" disabled={isSubmitting} className="w-full">
                        {isSubmitting ? "Resetting password..." : "Reset Password"}
                    </Button>
                </Field>
            </FieldGroup>
        </form>
    );
};

const validationSchema = z.object({
    email: z
        .string().min(1, { message: "Email is required" })
        .email({ message: "Must be a valid email", }),
    password: z
        .string()
        .min(6, { message: "Password must be at least 6 characters" }),
    confirmPassword: z
        .string()
        .min(1, { message: "Confirm Password is required" }),
})
    .refine((data) => data.password === data.confirmPassword, {
        path: ["confirmPassword"],
        message: "Password don't match",
    });
type FormModel = z.infer<typeof validationSchema>;
