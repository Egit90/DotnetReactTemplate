import { Link } from "react-router-dom"
import { Button } from "@/components/ui/button"

export function NotFound() {
    return (
        <div className="flex min-h-[80vh] flex-col items-center justify-center">
            <div className="text-center space-y-4">
                <h1 className="text-9xl font-bold text-muted-foreground">404</h1>
                <h2 className="text-3xl font-semibold">Page not found</h2>
                <p className="text-muted-foreground max-w-md">
                    Sorry, we couldn't find the page you're looking for.
                </p>
                <div className="flex gap-4 justify-center pt-4">
                    <Button asChild>
                        <Link to="/">Go back home</Link>
                    </Button>
                </div>
            </div>
        </div>
    )
}