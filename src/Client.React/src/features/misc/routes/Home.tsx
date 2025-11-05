import { Button } from "@/components/ui/button";
import { ArrowRight, Zap, Shield, Sparkles } from "lucide-react";

export const Home = () => {
    return (
        <main>
            {/* Hero Section */}
            <div className="lg:grid lg:grid-cols-12 lg:gap-8">
                <div className="sm:text-center md:max-w-2xl md:mx-auto lg:col-span-6 lg:text-left">
                    <h1 className="text-4xl font-bold text-foreground tracking-tight sm:text-5xl md:text-6xl">
                        Welcome to
                        <span className="block text-primary">Your Application</span>
                    </h1>
                    <p className="mt-3 text-base text-muted-foreground sm:mt-5 sm:text-xl lg:text-lg xl:text-xl">
                        Build amazing experiences with modern tools and best practices.
                        Get started quickly and focus on what matters most.
                    </p>
                    <div className="mt-8 sm:max-w-lg sm:mx-auto sm:text-center lg:text-left lg:mx-0">
                        <Button
                            size="lg"
                            className="text-lg rounded-full"
                        >
                            Get Started
                            <ArrowRight className="ml-2 h-5 w-5" />
                        </Button>
                    </div>
                </div>
                <div className="mt-12 relative sm:max-w-lg sm:mx-auto lg:mt-0 lg:max-w-none lg:mx-0 lg:col-span-6 lg:flex lg:items-center">
                    <div className="relative mx-auto w-full rounded-lg shadow-lg lg:max-w-md">
                        <div className="relative block w-full bg-white rounded-lg overflow-hidden p-8">
                            <div className="flex items-center justify-center h-64 bg-gradient-to-br from-primary/10 to-primary/5 rounded">
                                <Sparkles className="h-24 w-24 text-primary" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Features Section */}
            <section className="py-16 bg-background w-full">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="lg:grid lg:grid-cols-3 lg:gap-8">
                        <div>
                            <div className="flex items-center justify-center h-12 w-12 rounded-md bg-primary text-primary-foreground">
                                <Zap className="h-6 w-6" />
                            </div>
                            <div className="mt-5">
                                <h2 className="text-lg font-medium text-foreground">
                                    Fast & Efficient
                                </h2>
                                <p className="mt-2 text-base text-muted-foreground">
                                    Built with performance in mind. Optimized for speed and
                                    efficiency to deliver the best user experience.
                                </p>
                            </div>
                        </div>

                        <div className="mt-10 lg:mt-0">
                            <div className="flex items-center justify-center h-12 w-12 rounded-md bg-primary text-primary-foreground">
                                <Shield className="h-6 w-6" />
                            </div>
                            <div className="mt-5">
                                <h2 className="text-lg font-medium text-foreground">
                                    Secure & Reliable
                                </h2>
                                <p className="mt-2 text-base text-muted-foreground">
                                    Security-first approach with industry best practices.
                                    Your data and privacy are our top priorities.
                                </p>
                            </div>
                        </div>

                        <div className="mt-10 lg:mt-0">
                            <div className="flex items-center justify-center h-12 w-12 rounded-md bg-primary text-primary-foreground">
                                <Sparkles className="h-6 w-6" />
                            </div>
                            <div className="mt-5">
                                <h2 className="text-lg font-medium text-foreground">
                                    Modern & Intuitive
                                </h2>
                                <p className="mt-2 text-base text-muted-foreground">
                                    Clean, intuitive interface designed for the best user
                                    experience. Easy to use, powerful to master.
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* CTA Section */}
            <section className="py-16 bg-muted/30">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="lg:grid lg:grid-cols-2 lg:gap-8 lg:items-center">
                        <div>
                            <h2 className="text-3xl font-bold text-foreground sm:text-4xl">
                                Ready to get started?
                            </h2>
                            <p className="mt-3 max-w-3xl text-lg text-muted-foreground">
                                Join us today and discover how easy it can be to achieve your
                                goals. Everything you need is right here.
                            </p>
                        </div>
                        <div className="mt-8 lg:mt-0 flex justify-center lg:justify-end">
                            <Button
                                size="lg"
                                className="text-lg rounded-full"
                            >
                                Learn More
                                <ArrowRight className="ml-3 h-6 w-6" />
                            </Button>
                        </div>
                    </div>
                </div>
            </section>
        </main>
    );
};