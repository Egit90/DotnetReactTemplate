import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Link } from "react-router-dom"

export const Unauthorized = () => {
    return (
        <div className="flex items-center justify-center min-h-screen">
            <Card className="w-96">
                <CardHeader>
                    <CardTitle>Access Denied</CardTitle>
                </CardHeader>
                <CardContent>
                    <p className="mb-4">You don't have permission to access this page.</p>
                    <Button asChild>
                        <Link to="/">Go Home</Link>
                    </Button>
                </CardContent>
            </Card>
        </div>
    )
}