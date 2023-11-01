export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

// same as what will go into Paginationheader.cs

export class PaginatedResult<T> {   //T: Members
    result?: T;
    pagination?: Pagination;
}