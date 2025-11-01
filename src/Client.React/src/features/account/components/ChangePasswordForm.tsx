import { SubmitHandler, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from '../../../providers/AuthProvider.tsx';
import { extractApiErrors } from 'crystal-client/src/axios-utils.ts';
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

export const ChangePasswordForm = () => {
    const { register, handleSubmit, formState: { isSubmitting, errors }, reset } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const { authClient } = useAuth();

    const onSubmit: SubmitHandler<FormModel> = async (data) => {
        try {
            await authClient.changePassword({ password: data.password, newPassword: data.newPassword });
            toast.success("Password changed successfully");
            reset();
        } catch (error) {
            const errors = extractApiErrors(error) ?? ["Error occurred"];
            errors.forEach(err => toast.error(err));
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" method="POST" noValidate={true}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="password">Current Password</FieldLabel>
                    <Input
                        {...register("password")}
                        id="password"
                        type="password"
                        autoComplete="current-password"
                        required
                    />
                    {errors.password?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.password.message}
                        </p>
                    )}
                </Field>

                <Field>
                    <FieldLabel htmlFor="newPassword">New Password</FieldLabel>
                    <Input
                        {...register("newPassword")}
                        id="newPassword"
                        type="password"
                        autoComplete="new-password"
                        required
                    />
                    {errors.newPassword?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.newPassword.message}
                        </p>
                    )}
                </Field>

                <Field>
                    <FieldLabel htmlFor="confirmNewPassword">Confirm New Password</FieldLabel>
                    <Input
                        {...register("confirmNewPassword")}
                        id="confirmNewPassword"
                        type="password"
                        autoComplete="new-password"
                        required
                    />
                    {errors.confirmNewPassword?.message && (
                        <p className="mt-2 text-sm text-destructive">
                            {errors.confirmNewPassword.message}
                        </p>
                    )}
                </Field>

                <Field>
                    <Button type="submit" disabled={isSubmitting} className="w-full">
                        {isSubmitting ? "Changing password..." : "Change Password"}
                    </Button>
                </Field>
            </FieldGroup>
        </form>
    );
};

const validationSchema = z.object({
    password: z
        .string()
        .min(6, {message: "Password must be at least 6 characters"}),
    newPassword: z
        .string()
        .min(6, {message: "Password must be at least 6 characters"}),
    confirmNewPassword: z
        .string()
        .min(1, {message: "Confirm Password is required"}),
})
    .refine((data) => data.newPassword === data.confirmNewPassword, {
        path: ["confirmPassword"],
        message: "Password don't match",
    });
type FormModel = z.infer<typeof validationSchema>;