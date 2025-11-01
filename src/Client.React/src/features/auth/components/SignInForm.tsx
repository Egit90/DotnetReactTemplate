import { Link, useNavigate } from "react-router-dom";
import { SubmitHandler, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "../../../providers/AuthProvider.tsx";
import { extractApiErrors } from 'crystal-client/src/axios-utils.ts';
import { cn } from "@/lib/utils"
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
export function SignInForm({ className, ...props }: React.ComponentProps<"div">) {

    const navigate = useNavigate();
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });

    const { authClient } = useAuth();
    const onSubmit: SubmitHandler<FormModel> = async data => {
        try {
            await authClient.signIn({ email: data.email, password: data.password })
            navigate("/");
        } catch (error) {
            const errors = extractApiErrors(error) ?? ["Error occurred"];
            errors.forEach(err => {
                toast.error(err, { dismissible: true, duration: 10000 });
            });
        }
    };

    return (
        <div className={cn("flex flex-col gap-6", className)} {...props}>
            <Card>
                <CardHeader>
                    <CardTitle>Login to your account</CardTitle>
                    <CardDescription>
                        Enter your email below to login to your account
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
                                <div className="flex items-center">
                                    <FieldLabel htmlFor="password">Password</FieldLabel>
                                    <Link
                                        to="/forgot-password"
                                        className="ml-auto inline-block text-sm text-primary underline-offset-4 hover:underline"
                                    >
                                        Forgot your password?
                                    </Link>
                                </div>
                                <Input
                                    {...register("password")}
                                    id="password" type="password" autoComplete="current-password"
                                    required />
                            </Field>
                            <p className="mt-2 text-sm text-destructive" id="password-error">
                                {errors.password?.message}
                            </p>
                            <Field>
                                <Button type="submit" disabled={isSubmitting}>
                                    {isSubmitting ? "Logging in..." : "Login"}
                                </Button>
                                <Button variant="outline" type="button">
                                    Login with Google
                                </Button>
                                <FieldDescription className="text-center">
                                    Don&apos;t have an account? <Link to="/signup" className="text-primary hover:underline">Sign up</Link>
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
        .min(1, { message: "Password is required" }),
});
type FormModel = z.infer<typeof validationSchema>;
