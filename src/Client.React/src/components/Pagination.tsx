import { ChevronLeft, ChevronRight } from "lucide-react"
import { Button } from "./ui/button"
import { SetStateAction } from "react"

type PaginationProps = {
    page: number,
    pageSize: number,
    totalCount: number,
    setPage: (value: SetStateAction<number>) => void
    hasPreviousPage: boolean
    hasNextPage: boolean
    totalPages: number
}
export const Pagination = ({ page, pageSize, totalCount, setPage, hasPreviousPage, totalPages, hasNextPage }: PaginationProps) => {
    return (
        <div className="flex items-center justify-between px-2 py-4">
            <div className="text-sm text-muted-foreground">
                Showing <span className="font-medium">{(page - 1) * pageSize + 1}</span> to{" "}
                <span className="font-medium">{Math.min(page * pageSize, totalCount)}</span> of{" "}
                <span className="font-medium">{totalCount}</span> items
            </div>
            <div className="flex items-center gap-2">
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(page - 1)}
                    disabled={!hasPreviousPage}
                >
                    <ChevronLeft className="h-4 w-4 mr-1" />
                    Previous
                </Button>
                <div className="flex items-center gap-1">
                    <span className="text-sm">
                        Page {page} of {totalPages}
                    </span>
                </div>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setPage(page + 1)}
                    disabled={!hasNextPage}
                >
                    Next
                    <ChevronRight className="h-4 w-4 ml-1" />
                </Button>
            </div>
        </div>
    )
}