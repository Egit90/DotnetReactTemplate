import { Link, Outlet } from "react-router-dom";
import { useAuth } from "../../providers/AuthProvider.tsx";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { CircleIcon, User } from "lucide-react";
import { Suspense, useState } from "react";
import { ThemeToggle } from "@/components/ThemeToggle";


export default function MainLayout() {
    return (
        <section className="flex flex-col min-h-screen bg-background">
            <Header />
            <Outlet />
        </section>
    );
}

function UserMenu() {
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const { user } = useAuth();

    if (!user) {
        return (
            <>
                <Button asChild className="rounded-full">
                    <Link to="/signin">Sign In</Link>
                </Button>
            </>
        );
    }

    return (
        <DropdownMenu open={isMenuOpen} onOpenChange={setIsMenuOpen}>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-10 w-10 rounded-full border-2 border-green-200 hover:border-green-400 transition-colors">
                    <div className="flex items-center justify-center h-full w-full rounded-full bg-gradient-to-br from-green-400 to-emerald-600">
                        <User className="h-5 w-5 text-white" />
                    </div>
                    <span className="sr-only">User menu</span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-64">
                <div className="flex items-center gap-3 p-3 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950 dark:to-emerald-950 rounded-t-md">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-green-400 to-emerald-600">
                        <User className="h-5 w-5 text-white" />
                    </div>
                    <div className="flex flex-col space-y-1">
                        <p className="text-sm font-medium leading-none">Account</p>
                        <p className="text-xs text-muted-foreground truncate max-w-[180px]">{user.email}</p>
                    </div>
                </div>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                    <Link to="/profile" className="cursor-pointer">
                        <User className="mr-2 h-4 w-4" />
                        My Account
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild className="text-red-600 focus:text-red-600">
                    <Link to="/signout" className="cursor-pointer">
                        Sign out
                    </Link>
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}


function Header() {
    return (
        <header className="border-b border-border bg-background">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
                <Link to="/" className="flex items-center">
                    <CircleIcon className="h-6 w-6 text-primary" />
                    <span className="ml-2 text-xl font-semibold text-foreground">Crystal</span>
                </Link>
                <div className="flex items-center space-x-2">
                    <ThemeToggle />
                    <Suspense fallback={<div className="h-9" />}>
                        <UserMenu />
                    </Suspense>
                </div>
            </div>
        </header>
    );
}