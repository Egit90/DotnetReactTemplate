import { Link, useNavigate } from "react-router-dom";
import { SubmitHandler, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from '../../../providers/AuthProvider.tsx';
import { extractApiErrors } from 'crystal-client/src/axios-utils.ts';
import { toast } from "sonner";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card.tsx";
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/field.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { cn } from "@/lib/utils.ts";


export function SignUpForm({ className, ...props }: React.ComponentProps<"div">) {

    const navigate = useNavigate();

    const { authClient } = useAuth();
    const { register, handleSubmit, formState: { isSubmitting, errors } } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const onSubmit: SubmitHandler<FormModel> = async data => {
        try {
            const res = await authClient.signUp({ email: data.email, password: data.password });
            navigate("/signup/confirmation", { state: { ...res } });
        } catch (error) {
            const errors = extractApiErrors(error) ?? ["Error occurred"];
            errors.forEach(err => toast.error(err));
        }
    };

    return (
        <div className={cn("flex flex-col gap-6", className)} {...props}>
            <Card>
                <CardHeader>
                    <CardTitle>Create a new Account</CardTitle>
                    <CardDescription>
                        Enter your email below to create a new account
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form method="POST" action="#" onSubmit={handleSubmit(onSubmit)} noValidate={true}>
                        <FieldGroup>
                            <Field>
                                <FieldLabel htmlFor="email">Email</FieldLabel>
                                <Input
                                    {...register("email")}
                                    id="email"
                                    type="email"
                                    autoComplete="email"
                                    placeholder="m@example.com"
                                    required
                                />
                            </Field>
                            <p className="mt-2 text-sm text-destructive" id="email-error">
                                {errors.email?.message}
                            </p>
                            <Field>
                                <FieldLabel htmlFor="password">Password</FieldLabel>
                                <Input
                                    {...register("password")}
                                    id="password"
                                    type="password"
                                    autoComplete="new-password"
                                    required />
                            </Field>
                            <p className="mt-2 text-sm text-destructive" id="password-error">
                                {errors.password?.message}
                            </p>
                            <Field>
                                <FieldLabel htmlFor="confirmPassword">Confirm Password</FieldLabel>
                                <Input
                                    {...register("confirmPassword")}
                                    id="confirmPassword"
                                    type="password"
                                    autoComplete="new-password"
                                    required />
                            </Field>
                            <p className="mt-2 text-sm text-destructive" id="confirmPassword-error">
                                {errors.confirmPassword?.message}
                            </p>
                            <Field>
                                <Button type="submit" disabled={isSubmitting}>
                                    {isSubmitting ? "Creating account..." : "Sign Up"}
                                </Button>
                                <FieldDescription className="text-center">
                                    Already have an account? <Link to="/signin" className="text-primary hover:underline">Sign in</Link>
                                </FieldDescription>
                            </Field>
                        </FieldGroup>
                    </form>
                </CardContent>
            </Card>
        </div>
    )
}

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