import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { SubmitHandler, useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { extractApiErrors } from 'crystal-client/src/axios-utils.ts';
import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Field,
    FieldDescription,
    FieldGroup,
    FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { toast } from "sonner";
import { ArrowLeft, House, Mail, CheckCircle2 } from "lucide-react";
import { useState } from "react";
import { useAuth } from "@/providers/AuthProvider";


export const ForgotPassword = () => {
    return (
        <>
            <div className="flex  min-w-full items-center justify-center relative">
                <Link
                    to="/"
                    className="absolute top-4 left-4 flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
                >
                    <House className="h-4 w-4" />
                    Home
                </Link>
                <div className="w-full max-w-md">
                    <ForgotPasswordComp />
                </div>
            </div>
        </>
    );
};


export const ForgotPasswordComp = () => {
    const { authClient } = useAuth();
    const [emailSent, setEmailSent] = useState(false);
    const [sentEmail, setSentEmail] = useState("");

    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });

    const onSubmit: SubmitHandler<FormModel> = async (data) => {
        try {
            await authClient.account.forgotPassword(data.email);
            setSentEmail(data.email);
            setEmailSent(true);
            toast.success("Password reset email sent!", {
                description: "Check your inbox for further instructions",
                duration: 5000
            });
        } catch (error) {
            const errors = extractApiErrors(error) ?? ["Failed to send reset email"];
            errors.forEach(err => {
                toast.error(err, { dismissible: true, duration: 10000 });
            });
        }
    };

    if (emailSent) {
        return (
            <Card>
                <CardHeader className="text-center">
                    <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
                        <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-400" />
                    </div>
                    <CardTitle>Check your email</CardTitle>
                    <CardDescription>
                        We've sent password reset instructions to
                    </CardDescription>
                    <p className="text-sm font-medium text-foreground mt-2">
                        {sentEmail}
                    </p>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="rounded-lg bg-muted p-4">
                        <p className="text-sm text-muted-foreground">
                            <Mail className="inline h-4 w-4 mr-2" />
                            Didn't receive the email? Check your spam folder or try again.
                        </p>
                    </div>
                    <div className="flex flex-col gap-2">
                        <Button
                            variant="outline"
                            onClick={() => setEmailSent(false)}
                            className="w-full"
                        >
                            Try another email
                        </Button>
                        <Button
                            variant="ghost"
                            asChild
                            className="w-full"
                        >
                            <Link to="/signin">
                                <ArrowLeft className="h-4 w-4 mr-2" />
                                Back to login
                            </Link>
                        </Button>
                    </div>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Forgot your password?</CardTitle>
                <CardDescription>
                    Enter your email address and we'll send you instructions to reset your password
                </CardDescription>
            </CardHeader>
            <CardContent>
                <form method="POST" action="#" onSubmit={handleSubmit(onSubmit)} noValidate={true}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor="email">Email address</FieldLabel>
                            <Input
                                {...register("email")}
                                id="email"
                                type="email"
                                autoComplete="email"
                                placeholder="m@example.com"
                                required
                            />
                            {errors.email?.message && (
                                <p className="mt-2 text-sm text-destructive">
                                    {errors.email.message}
                                </p>
                            )}
                        </Field>
                        <Field>
                            <Button type="submit" disabled={isSubmitting} className="w-full">
                                {isSubmitting ? "Sending..." : "Send reset instructions"}
                            </Button>
                            <FieldDescription className="text-center">
                                Remember your password? <Link to="/signin" className="text-primary hover:underline">Sign in</Link>
                            </FieldDescription>
                        </Field>
                    </FieldGroup>
                </form>
            </CardContent>
        </Card>
    );
};

const validationSchema = z.object({
    email: z
        .string().min(1, { message: "Email is required" })
        .email({ message: "Must be a valid email" })
});
type FormModel = z.infer<typeof validationSchema>;